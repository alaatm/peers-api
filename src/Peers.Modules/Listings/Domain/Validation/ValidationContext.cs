using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Snapshots;

namespace Peers.Modules.Listings.Domain.Validation;

internal sealed class ValidationContext
{
    public required Listing Listing { get; init; }

    public ProductType ProductType => Listing.ProductType;
    public List<ListingVariant> Variants => Listing.Variants;
    public VariantAxesSnapshot AxesSnapshot => Listing.AxesSnapshot;

    public Dictionary<string, AttributeDefinition> DefByKey { get; }
    public Dictionary<string, VariantAxisSnapshot> AxisByDefKey { get; }

    [SetsRequiredMembers]
    public ValidationContext(Listing listing)
    {
        Listing = listing;
        DefByKey = ProductType.Attributes.ToDictionary(p => p.Key, StringComparer.Ordinal);
        AxisByDefKey = AxesSnapshot.Axes.ToDictionary(p => p.DefinitionKey, p => p, StringComparer.Ordinal);
    }
}
