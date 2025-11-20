using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Listings.Domain.Validation;

namespace Peers.Modules.Listings.Domain;

public partial class Listing
{
    private void Validate(ValidationContext ctx)
    {
        if (Variants.Count == 0)
        {
            throw StateError("Expected at least one variant.");
        }

        // All required header attributes must be present
        foreach (var attr in ProductType.Attributes)
        {
            if (attr.IsRequired && !attr.IsVariant)
            {
                if (Attributes.Find(p => p.AttributeDefinition == attr) is null)
                {
                    throw StateError($"Missing required attribute '{attr}'.");
                }
            }
        }

        foreach (var attr in Attributes)
        {
            attr.Validate(ctx);
        }

        try
        {
            FulfillmentPreferences.Validate(ProductType.Kind, ShippingProfile);
        }
        catch (DomainException ex)
        {
            throw StateError("Invalid fulfillment preferences.", ex);
        }

        Snapshot.Validate(ctx);
        foreach (var variant in Variants)
        {
            variant.Validate(ctx);
        }

        InvalidDomainStateException StateError(string message, Exception? inner = null) => inner is null
            ? throw new InvalidDomainStateException(this, message)
            : throw new InvalidDomainStateException(this, message, inner);
    }
}
