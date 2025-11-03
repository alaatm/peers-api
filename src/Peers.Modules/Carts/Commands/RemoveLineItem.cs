using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Ordering.Domain;

namespace Peers.Modules.Carts.Commands;

public static class RemoveLineItem
{
    /// <summary>
    /// Removes a line item for a specific product variant from the shopping cart.
    /// </summary>
    /// <param name="ListingId">The unique identifier of the product listing to remove.</param>
    /// <param name="VariantKey">The key that identifies the specific variant of the product to remove. Cannot be null or empty.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        int ListingId,
        string VariantKey) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.ListingId).GreaterThan(0);
            RuleFor(p => p.VariantKey).NotEmpty();
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IIdentityInfo _identity;
        private readonly ILogger<Handler> _log;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider,
            IIdentityInfo identity,
            ILogger<Handler> log)
        {
            _context = context;
            _timeProvider = timeProvider;
            _identity = identity;
            _log = log;
        }

        public Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk) => CartMutationOperation.ExecuteAsync(
            _context,
            _timeProvider,
            _identity,
            _log,
            cmd.ListingId,
            cmd.VariantKey,
            (cart, listing) => cart.RemoveLineItem(listing, cmd.VariantKey, _timeProvider.UtcNow()),
            ctk);
    }
}
