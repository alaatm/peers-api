namespace Peers.Modules.Users.Commands;

public static class DeleteAccount
{
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command() : ICommand;

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IIdentityInfo _identity;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider,
            IIdentityInfo identity)
        {
            _context = context;
            _timeProvider = timeProvider;
            _identity = identity;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var user = await _context
                .Users
                .Include(p => p.RefreshTokens)
                .Include(p => p.StatusChangeHistory)
                .Include(p => p.DeviceList)
                .FirstAsync(p => p.Id == _identity.Id, ctk);

            var customer = await _context
                .Customers
                .FirstAsync(p => p.Id == user.Id, ctk);

            customer.DeleteAccount(_timeProvider.UtcNow());

            await _context.SaveChangesAsync(ctk);
            return Result.NoContent();
        }
    }
}
