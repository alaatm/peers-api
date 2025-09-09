using Humanizer;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.Commands;

public static class ChangeStatus
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="Id">The id of the user.</param>
    /// <param name="NewStatus">The new status.</param>
    /// <param name="ChangeReason">The change reason.</param>
    /// <returns></returns>
    [Authorize(Roles = Roles.UsersManager)]
    public sealed record Command(
        int Id,
        UserStatus NewStatus,
        string ChangeReason) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _newStatus = nameof(Command.NewStatus).Humanize();
        private static readonly string _changeReason = nameof(Command.ChangeReason).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Id).GreaterThan(0);
            RuleFor(p => p.NewStatus).IsInEnum().WithName(l[_newStatus]);
            RuleFor(p => p.ChangeReason).NotEmpty().WithName(l[_changeReason]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider,
            IIdentityInfo identity,
            IStrLoc l)
        {
            _context = context;
            _timeProvider = timeProvider;
            _identity = identity;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _context
                .Users
                .Include(p => p.RefreshTokens)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not AppUser user)
            {
                return Result.BadRequest(_l["User not found."]);
            }

            var manager = await _context
                .Users
                .AsNoTracking()
                .FirstAsync(p => p.Id == _identity.Id, ctk);

            user.ChangeStatus(_timeProvider.UtcNow(), manager, cmd.NewStatus, cmd.ChangeReason);
            await _context.SaveChangesAsync(ctk);
            return Result.NoContent();
        }
    }
}
