using System.Diagnostics;
using Humanizer;
using Peers.Core.Communication.Push;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Security.StrongKeys;
using Peers.Modules.Users.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace Peers.Modules.Users.Commands;

public static class RegisterDevice
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="Id">The device id.</param>
    /// <param name="Manufacturer">The device manufacturer.</param>
    /// <param name="Model">The device model.</param>
    /// <param name="Platform">The device platform.</param>
    /// <param name="OSVersion">The OS version number.</param>
    /// <param name="Idiom">The device idiom.</param>
    /// <param name="Type">The device type, physical or virtual.</param>
    /// <param name="PnsHandle">The PNS handle.</param>
    /// <param name="App"></param>
    /// <param name="AppVersion"></param>
    /// <param name="TrackingId"></param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        Guid Id,
        string Manufacturer,
        string Model,
        string Platform,
        string OSVersion,
        string Idiom,
        string Type,
        string PnsHandle,
        string App,
        string AppVersion,
        string? TrackingId) : ICommand, IValidatable;

    public sealed record Response(string TrackingId);

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _drId = nameof(Command.Id).Humanize();
        private static readonly string _drManufacturer = nameof(Command.Manufacturer).Humanize();
        private static readonly string _drModel = nameof(Command.Model).Humanize();
        private static readonly string _drPlatform = nameof(Command.Platform).Humanize();
        private static readonly string _drOSVersion = nameof(Command.OSVersion).Humanize();
        private static readonly string _drIdiom = nameof(Command.Idiom).Humanize();
        private static readonly string _drType = nameof(Command.Type).Humanize();
        private static readonly string _drPnsHandle = nameof(Command.PnsHandle).Humanize();
        private static readonly string _drApp = nameof(Command.App).Humanize();
        private static readonly string _drAppVersion = nameof(Command.AppVersion).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Id).NotEmpty().WithName(_drId);
            RuleFor(p => p.Manufacturer).NotEmpty().WithName(l[_drManufacturer]);
            RuleFor(p => p.Model).NotEmpty().WithName(l[_drModel]);
            RuleFor(p => p.Platform).NotEmpty().WithName(l[_drPlatform]);
            RuleFor(p => p.OSVersion).NotEmpty().WithName(l[_drOSVersion]);
            RuleFor(p => p.Idiom).NotEmpty().WithName(l[_drIdiom]);
            RuleFor(p => p.Type).NotEmpty().WithName(l[_drType]);
            RuleFor(p => p.PnsHandle).NotEmpty().WithName(l[_drPnsHandle]);
            RuleFor(p => p.App).NotEmpty().WithName(l[_drApp]);
            RuleFor(p => p.AppVersion).NotEmpty().WithName(l[_drAppVersion]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IMemoryCache _cache;
        private readonly IFirebaseMessagingService _firebase;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;
        private readonly ILogger<Handler> _log;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider,
            IMemoryCache cache,
            IFirebaseMessagingService firebase,
            IIdentityInfo identity,
            IStrLoc l,
            ILogger<Handler> log)
        {
            _context = context;
            _timeProvider = timeProvider;
            _cache = cache;
            _firebase = firebase;
            _identity = identity;
            _l = l;
            _log = log;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var user = await _context
                .Users
                .Include(p => p.DeviceList)
                .FirstAsync(p => p.Id == _identity.Id, ctk);

            var device = user.DeviceList.Find(p => p.DeviceId == cmd.Id) ?? await _context
                .Set<Device>()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.DeviceId == cmd.Id, ctk);

            if (device is null)
            {
                if (cmd.TrackingId is not null)
                {
                    if (!_cache.TryGetValue<Guid>(cmd.TrackingId, out var oldDeviceId))
                    {
                        _log.DeviceRegisterTrackingInvalidTracking(cmd.TrackingId, cmd.Id);
                        return Result.BadRequest(_l["Invalid device registration tracking request. Tracking id not found."]);
                    }

                    var oldDevice = await _context
                        .Set<Device>()
                        .Include(p => p.User)
                        .FirstOrDefaultAsync(p => p.DeviceId == oldDeviceId, ctk);

                    if (oldDevice is null)
                    {
                        _log.DeviceRegisterTrackingNonExistingDevice(cmd.TrackingId, cmd.Id, oldDeviceId);
                        return Result.BadRequest(_l["Invalid device registration tracking request. Device not found."]);
                    }

                    if (oldDevice.PnsHandle == cmd.PnsHandle)
                    {
                        _log.DeviceRegisterTrackingSameToken(cmd.TrackingId, cmd.Id, oldDeviceId);
                        return Result.BadRequest(_l["Invalid device registration tracking request. PNS handle needs update."]);
                    }

                    await _firebase.UnsubscribeUserTopicAsync(user, oldDevice.PnsHandle);
                    oldDevice.User.UnlinkDevice(oldDevice);
                    Debug.Assert(oldDeviceId != cmd.Id);
                }

                device = user.RegisterDevice(
                    _timeProvider.UtcNow(),
                    cmd.Id,
                    cmd.Manufacturer,
                    cmd.Model,
                    cmd.Platform,
                    cmd.OSVersion,
                    cmd.Idiom,
                    cmd.Type,
                    cmd.PnsHandle,
                    cmd.App,
                    cmd.AppVersion);

                await _context.SaveChangesAsync(ctk);
                await _firebase.SubscribeUserTopicAsync(user, cmd.PnsHandle);
                return Result.Created(value: new IdObj(device.Id));
            }
            else
            {
                if (cmd.TrackingId is not null)
                {
                    _log.DeviceRegisterUnexpectedTracking(cmd.TrackingId);
                    return Result.BadRequest(_l["Invalid device registration tracking request. A tracking request was not expected."]);
                }

                var prevOwner = device.User;

                // The device already exists. This can happen when:
                //  - The same user logs into a device that he previously used. (enrolled/logged in).
                //  - Two different users login to the same device.

                if (prevOwner == user)
                {
                    // Same user, this is usually a logout/login scenario. Just update app version and pns handle if needed.
                    device.UpdateAppVersion(cmd.AppVersion);
                    device.UpdateHandle(_timeProvider.UtcNow(), cmd.PnsHandle);
                    await _context.SaveChangesAsync(ctk);

                    return Result.NoContent();
                }
                else
                {
                    // Different user, this is a device ownership switch. We need to unlink the current device from the previous owner
                    // and request a new device id be generated on the client along with a new PNS handle.
                    // This scenario is typically when the same person has 2 accounts and wants to switch between them on the same phone.
                    var trackingId = KeyGenerator.Create(8);
                    _cache.Set(trackingId, cmd.Id, TimeSpan.FromSeconds(15));
                    return Result.Accepted(value: new Response(trackingId));
                }
            }
        }
    }
}
