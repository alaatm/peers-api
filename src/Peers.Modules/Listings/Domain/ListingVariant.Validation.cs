using Peers.Core.Domain.Errors;
using Peers.Modules.Listings.Domain.Validation;

namespace Peers.Modules.Listings.Domain;

public partial class ListingVariant
{
    internal void Validate(ValidationContext ctx)
    {
        // Basic fields validation
        if (string.IsNullOrWhiteSpace(VariantKey))
        {
            throw StateError("VariantKey is empty.");
        }

        if (string.IsNullOrWhiteSpace(SkuCode))
        {
            throw StateError("SkuCode is empty.");
        }

        if (Price < 0)
        {
            throw StateError("Price is negative.");
        }

        if (StockQty is < 0)
        {
            throw StateError("StockQty is negative.");
        }
        //

        //if (Attributes is null || Attributes.Count == 0)
        //{
        //    throw StateError("Missing or no attribute definitions defined.");
        //}

        // No duplicate defs
        if (Attributes.Select(a => a.AttributeDefinition).Distinct().Count() != Attributes.Count)
        {
            throw StateError("Duplicate attribute definitions found.");
        }

        // All required variant attributes must be present
        foreach (var attr in ctx.ProductType.Attributes)
        {
            if (attr.IsRequired && attr.IsVariant)
            {
                if (Attributes.Find(p => p.AttributeDefinition == attr) is null)
                {
                    throw StateError($"Missing required variant attribute '{attr}'.");
                }
            }
        }

        // Per-attribute validation
        foreach (var attr in Attributes)
        {
            attr.Validate(ctx);
        }

        // Snapshot validation
        SelectionSnapshot.Validate(ctx);

        // Logistics validation
        try
        {
            Logistics?.Validate();
        }
        catch (DomainException ex)
        {
            throw StateError("Logistics profile is invalid.", ex);
        }

        InvalidDomainStateException StateError(string message, Exception? inner = null) => inner is null
            ? throw new InvalidDomainStateException(this, message)
            : throw new InvalidDomainStateException(this, message, inner);
    }
}
