using System.Globalization;
using Microsoft.AspNetCore.Http.HttpResults;
using Peers.Core.Background;
using Peers.Core.Communication.Push;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.SystemInfo.Queries;
using Peers.Modules.Users.Events;
using Microsoft.Extensions.Caching.Memory;

namespace Peers.Modules.Users.Commands;

/// <summary>
/// This is a proxy command that will call other commands and queries to initialize the calling client app.
/// </summary>
public static class Initialize
{
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        RegisterDevice.Command RegisterDeviceCmd) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(p => p.RegisterDeviceCmd).NotNull();
    }

    public sealed record Response(
        ListSupportedLanguages.Response[] SupportedLanguages,
        Response.RegisterDeviceResponse DeviceResponse,
        string? UpdateLink,
        string? LatestVersion)
    {
        public sealed record RegisterDeviceResponse(int Status, string? TrackingId);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IMemoryCache _cache;
        private readonly IFirebaseMessagingService _firebase;
        private readonly IIdentityInfo _identity;
        private readonly IProducer _producer;
        private readonly ILogger<RegisterDevice.Handler> _log;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider,
            IMemoryCache cache,
            IFirebaseMessagingService firebase,
            IIdentityInfo identity,
            IProducer producer,
            ILogger<RegisterDevice.Handler> log,
            IStrLoc l)
        {
            _context = context;
            _timeProvider = timeProvider;
            _cache = cache;
            _firebase = firebase;
            _identity = identity;
            _producer = producer;
            _log = log;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            await _producer.PublishAsync(new AppOpened(_timeProvider.UtcNow(), _identity, _identity.Id), ctk);

            var deviceResponse = FromRegisterDeviceResponseResult(
                await new RegisterDevice
                    .Handler(_context, _timeProvider, _cache, _firebase, _identity, _l, _log)
                    .Handle(cmd.RegisterDeviceCmd, ctk));

            var (updateLink, latestVersion) = await GetUpdateLinkAsync(cmd.RegisterDeviceCmd);

            return Result.Ok(new Response(
                ListSupportedLanguages.Response.FromSysLanguages(),
                deviceResponse,
                updateLink,
                latestVersion));
        }

        private async Task<(string?, string?)> GetUpdateLinkAsync(RegisterDevice.Command cmd)
        {
            string? updateLink = null;
            var platform = cmd.Platform;
            var package = cmd.App;
            var version = cmd.AppVersion;
            var clientApp = await _context.ClientApps.FirstOrDefaultAsync();

            if (clientApp is not null && TryParse(version, out var current) && current < clientApp.LatestVersion.Version)
            {
                updateLink = clientApp.GetStoreLink(platform);
            }

            return (updateLink, clientApp?.LatestVersion.VersionString);
        }

        private bool TryParse(string version, out Version? result)
        {
            var match = RegexStatic.ClientVersionRegex().Match(version);
            if (match.Success)
            {
                var major = int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture);
                var minor = int.Parse(match.Groups["minor"].Value, CultureInfo.InvariantCulture);
                var build = int.Parse(match.Groups["build"].Value, CultureInfo.InvariantCulture);
                var revision = int.Parse(match.Groups["revision"].Value, CultureInfo.InvariantCulture);

                result = new Version(major, minor, build, revision);
                return true;
            }

            _log.ClientVersionParseFailed(version);

            result = null;
            return false;
        }
    }

    internal static Response.RegisterDeviceResponse FromRegisterDeviceResponseResult(IResult result) => result switch
    {
        Created<IdObj> created => new Response.RegisterDeviceResponse(201, null),
        NoContent => new Response.RegisterDeviceResponse(204, null),
        Accepted<RegisterDevice.Response> accepted => new Response.RegisterDeviceResponse(202, accepted.Value!.TrackingId),
        IStatusCodeHttpResult sc => new Response.RegisterDeviceResponse(sc.StatusCode ?? 400, null),
        _ => new Response.RegisterDeviceResponse(500, null),
    };
}
