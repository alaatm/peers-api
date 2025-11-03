using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Ordering.Domain;

namespace Peers.Modules.Carts.Commands;

public static class UpdateLineItemQuantity
{
    /// <summary>
    /// Uupdates the quantity of a specific product variant in the shopping cart.
    /// </summary>
    /// <param name="ListingId">The unique identifier of the product listing to update.</param>
    /// <param name="VariantKey">The key that identifies the specific variant of the product to update. Cannot be null or empty.</param>
    /// <param name="NewQuantity">The new quantity to set for the specified product variant. Must be zero or a positive integer.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        int ListingId,
        string VariantKey,
        int NewQuantity) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.ListingId).GreaterThan(0);
            RuleFor(p => p.VariantKey).NotEmpty();
            RuleFor(p => p.NewQuantity).GreaterThanOrEqualTo(0);
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
            (cart, listing) => cart.UpdateLineItemQuantity(listing, cmd.VariantKey, cmd.NewQuantity, _timeProvider.UtcNow()),
            ctk);
    }
}
