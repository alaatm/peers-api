using Humanizer;
using Peers.Core.Communication.Push;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization;

namespace Peers.Modules.Users.Commands;

public static class SetPreferredLanguage
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="PreferredLanguage">The user's preferred language.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(string PreferredLanguage) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _preferredLanguage = nameof(Command.PreferredLanguage).Humanize();

        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.PreferredLanguage)
                .NotEmpty()
                .Length(2)
                .WithName(l[_preferredLanguage]);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly IFirebaseMessagingService _firebase;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            IFirebaseMessagingService firebase,
            IIdentityInfo identity,
            IStrLoc l)
        {
            _context = context;
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

            if (!Lang.SupportedLanguages.Contains(cmd.PreferredLanguage))
            {
                return Result.BadRequest(_l["Invalid language code."]);
            }

            foreach (var device in user.DeviceList)
            {
                await _firebase.UnsubscribeUserTopicAsync(user, device.PnsHandle);
            }
            user.SetPreferredLanguage(cmd.PreferredLanguage);
            foreach (var device in user.DeviceList)
            {
                await _firebase.SubscribeUserTopicAsync(user, device.PnsHandle);
            }

            await _context.SaveChangesAsync(ctk);
            return Result.NoContent();
        }
    }
}
