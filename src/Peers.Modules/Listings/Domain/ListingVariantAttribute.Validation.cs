using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Validation;

namespace Peers.Modules.Listings.Domain;

public partial class ListingVariantAttribute
{
    internal void Validate(ValidationContext ctx)
    {
        var pt = ctx.ProductType;
        var def = AttributeDefinition ?? throw StateError("AttributeDefinition is null.");

        if (def.ProductType != pt)
        {
            throw StateError($"Attribute definition '{def.D}' does not belong to product type '{pt.D}'.");
        }

        // Group-kind must never appear here
        if (def.Kind is AttributeKind.Group)
        {
            throw StateError($"Attribute definition '{def.D}' is of kind 'Group' and cannot be used in a ListingVariantAttribute.");
        }

        // Must be a variant attribute except group members
        if (!def.IsVariant)
        {
            if (def is not NumericAttributeDefinition n || n.GroupDefinition is null)
            {
                throw StateError($"Attribute definition '{def.D}' is not a variant attribute and cannot be set on a ListingVariantAttribute.");
            }
        }

        // Position non-negative
        if (Position < 0)
        {
            throw StateError($"Attribute definition '{def.D}' has invalid negative position '{Position}'.");
        }

        // Only single value set
        if ((NumericValue is not null ? 1 : 0) +
            (EnumAttributeOption is not null ? 1 : 0) +
            (LookupOption is not null ? 1 : 0) != 1)
        {
            throw StateError($"Attribute definition '{def.D}' must have exactly one of NumericValue, EnumAttributeOption, or LookupOption set.");
        }

        // Value-shape vs kind
        switch (def)
        {
            case EnumAttributeDefinition ed:
            {
                if (EnumAttributeOption is not { } opt)
                {
                    throw StateError($"Attribute definition '{def.D}' expects an enum option to be set.");
                }
                if (opt.EnumAttributeDefinition != ed)
                {
                    throw StateError($"Enum option '{opt.D}' does not belong to attribute definition '{def.D}'.");
                }

                break;
            }
            case LookupAttributeDefinition ld:
            {
                if (LookupOption is not { } opt)
                {
                    throw StateError($"Attribute definition '{def.D}' expects a lookup option to be set.");
                }
                if (opt.Type != ld.LookupType)
                {
                    throw StateError($"Lookup option '{opt.D}' does not belong to the lookup type '{ld.LookupType.D}' associated with attribute definition '{def.D}'.");
                }
                if (!ld.IsOptionAllowed(opt, noEntriesMeansAllowAll: true))
                {
                    throw StateError($"Lookup option '{opt.D}' is not allowed for attribute '{ld.D}'.");
                }

                break;
            }
            case NumericAttributeDefinition nd:
            {
                if (NumericValue is not { } num)
                {
                    throw StateError($"Attribute definition '{def.D}' expects a numeric value to be set.");
                }

                try
                {
                    nd.ValidateValue(num);
                }
                catch (DomainException ex)
                {
                    throw StateError($"Numeric value '{num}' is invalid for attribute definition '{def.D}'", ex);
                }

                break;
            }
            default:
                throw StateError($"Unsupported variant kind '{def.Kind}' for attribute definition '{def.D}'.");
        }

        InvalidDomainStateException StateError(string message, Exception? inner = null) => inner is null
            ? throw new InvalidDomainStateException(this, message)
            : throw new InvalidDomainStateException(this, message, inner);
    }
}
