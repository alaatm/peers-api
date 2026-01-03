using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Listings.Domain.Translations;

namespace Peers.Modules.Listings.Commands;

public static class CreateListing
{
    /// <summary>
    /// Creates a product listing with specified fulfillment preferences, pricing, and localized names.
    /// </summary>
    /// <param name="ProductTypeId">The identifier of the product type to associate with the listing.</param>
    /// <param name="Fulfillment">The fulfillment preferences that determine how the product will be delivered or made available to buyers.</param>
    /// <param name="ShippingProfileId">The identifier of the shipping profile to use for this listing, or null if no shipping profile is associated.</param>
    /// <param name="Hashtag">An optional hashtag to categorize or tag the listing. Can be null if no hashtag is specified.</param>
    /// <param name="Price">The price of the product listing. Must be a non-negative value.</param>
    /// <param name="Names">An array of localized names and descriptions for the listing. Each entry provides display information for a specific language.</param>
    [Authorize(Roles = Roles.Seller)]
    public sealed record Command(
        int ProductTypeId,
        FulfillmentPreferences Fulfillment,
        int? ShippingProfileId,
        string? Hashtag,
        decimal Price,
        ListingTr.Dto[] Names) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.ProductTypeId).GreaterThan(0);
            RuleFor(p => p.Names).NotNull().NotEmpty().Must(p => p.FirstOrDefault(p => p.LangCode == "en") is not null)
                .WithMessage(l["At least one name in English (en) is required."]);
        }
    }

    /// <summary>
    /// Represents the result of the create listing operation.
    /// </summary>
    /// <param name="ListingId">The unique identifier of the created listing.</param>
    /// <param name="SnapshotId">
    /// The unique identifier of the listing snapshot.
    /// This value must be passed for further updates on the listing.
    /// </param>
    public sealed record Response(
        int ListingId,
        string SnapshotId);

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
            if (await _context
                .ProductTypes
                .FirstOrDefaultAsync(p => p.Id == cmd.ProductTypeId, ctk) is not { } pt)
            {
                return Result.BadRequest(detail: "Invalid product type.");
            }

            var seller = await _context
                .Sellers
                .Include(p => p.ShippingProfiles.Where(p => p.Id == cmd.ShippingProfileId))
                .FirstAsync(s => s.Id == _identity.Id, ctk);

            var shippingProfile = seller.ShippingProfiles.FirstOrDefault();

            if (cmd.ShippingProfileId is not null &&
                shippingProfile is null)
            {
                return Result.BadRequest(detail: "Invalid shipping profile.");
            }


            var title = cmd.Names.GetEn()!.Title;
            var descr = cmd.Names.GetEn()!.Description;

            var listing = Listing.Create(title, seller, pt, cmd.Fulfillment, shippingProfile, descr, cmd.Hashtag, cmd.Price, _timeProvider.UtcNow());
            listing.UpsertTranslations(cmd.Names);
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync(ctk);

            return Result.Created(value: new Response(listing.Id, listing.Snapshot.SnapshotId));
        }
    }
}
