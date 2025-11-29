using Peers.Core.Cqrs.Pipeline;

namespace Peers.Modules.Carts.Commands;

public static class AddLineItem
{
    /// <summary>
    /// Adds a line item for a specific product variant and quantity to the shopping cart.
    /// </summary>
    /// <param name="ListingId">The unique identifier of the product listing to order.</param>
    /// <param name="VariantKey">The key that identifies the specific variant of the product to order. Cannot be null or empty.</param>
    /// <param name="Quantity">The number of units to order. Must be greater than zero.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        int ListingId,
        string VariantKey,
        int Quantity) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.ListingId).GreaterThan(0);
            RuleFor(p => p.VariantKey).NotEmpty();
            RuleFor(p => p.Quantity).GreaterThan(0);
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

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk) => await CartMutationOperation.ExecuteAsync(
            _context,
            _timeProvider,
            _identity,
            _log,
            cmd.ListingId,
            cmd.VariantKey,
            (cart, listing) => cart.AddLineItem(listing, cmd.VariantKey, cmd.Quantity, _timeProvider.UtcNow()),
            ctk);
    }
}
