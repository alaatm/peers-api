using NetTopologySuite.Geometries;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Sellers.Domain;

namespace Peers.Modules.Sellers.Commands;

public static class CreateShippingProfile
{
    /// <summary>
    /// Creates a new shipping profile for the seller.
    /// </summary>
    /// <param name="Name">The name of the profile.</param>
    /// <param name="OriginLocation">The geographic origin point from which shipments are dispatched. Specifies where the shipping rate and policy will apply.</param>
    /// <param name="Rate">The seller-managed shipping rate to be applied for shipments originating from the specified location.</param>
    /// <param name="FreeShippingPolicy">The free shipping policy associated with the command, determining eligibility and conditions for free shipping.</param>
    [Authorize(Roles = Roles.Seller)]
    public sealed record Command(
        string Name,
        Point OriginLocation,
        SellerManagedRate Rate,
        FreeShippingPolicy FreeShippingPolicy) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.OriginLocation).NotNull();
            RuleFor(p => p.Rate).NotNull();
            RuleFor(p => p.FreeShippingPolicy).NotNull();
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            IIdentityInfo identity,
            IStrLoc l)
        {
            _context = context;
            _identity = identity;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var seller = await _context.Sellers
                .Include(p => p.ShippingProfiles)
                .FirstAsync(s => s.Id == _identity.Id, ctk);

            var shippingProfile = seller.CreateShippingProfile(cmd.Name, cmd.OriginLocation, cmd.Rate, cmd.FreeShippingPolicy);
            await _context.SaveChangesAsync(ctk);

            return Result.Created(value: new IdObj(shippingProfile.Id));
        }
    }
}
