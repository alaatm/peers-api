using Humanizer;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Core.Cqrs.Pipeline;

namespace Mashkoor.Modules.Users.Commands;

public static class UpdatePnsHandle
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="DeviceId">The device id with the updated handle.</param>
    /// <param name="PnsHandle">The updated handle. Can be null to indicate request to remove the token (usually on logout).</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        Guid DeviceId,
        string? PnsHandle) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _deviceId = nameof(Command.DeviceId).Humanize();
        private static readonly string _pnsHandle = nameof(Command.PnsHandle).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.DeviceId).NotEmpty().WithName(l[_deviceId]);
            When(p => p.PnsHandle is not null, () => RuleFor(p => p.PnsHandle).NotEmpty().WithName(l[_pnsHandle]));
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IFirebaseMessagingService _firebase;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext context,
            TimeProvider timeProvider,
            IFirebaseMessagingService firebase,
            IIdentityInfo identity,
            IStrLoc l)
        {
            _context = context;
            _timeProvider = timeProvider;
            _firebase = firebase;
            _identity = identity;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var user = await _context
                .Users
                .Include(p => p.DeviceList)
                .FirstAsync(p => p.Id == _identity.Id, ctk);

            if (user.UpdatePnsHandle(_timeProvider.UtcNow(), cmd.DeviceId, cmd.PnsHandle, out var oldHandle))
            {
                await _firebase.UnsubscribeUserTopicAsync(user, oldHandle);
                await _firebase.SubscribeUserTopicAsync(user, cmd.PnsHandle);

                await _context.SaveChangesAsync(ctk);
                return Result.NoContent();
            }

            return Result.BadRequest(_l["The specified device does not exist."]);
        }
    }
}
