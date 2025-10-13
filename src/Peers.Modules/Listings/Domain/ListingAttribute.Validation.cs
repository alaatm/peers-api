using System.Globalization;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Validation;

namespace Peers.Modules.Listings.Domain;

public partial class ListingAttribute
{
    internal void Validate(ValidationContext ctx)
    {
        var pt = ctx.ProductType;
        var def = AttributeDefinition ?? throw StateError("AttributeDefinition is null.");

        if (def.ProductType != pt)
        {
            throw StateError($"Attribute definition '{def.D}' does not belong to product type '{pt.D}'.");
        }

        // Variants should not be present here (header only)
        if (def.IsVariant)
        {
            throw StateError($"Attribute definition '{def.D}' is a variant attribute and cannot be set at listing header.");
        }

        // Numeric members of a group cannot be set independently
        if (def is NumericAttributeDefinition n &&
            n.GroupDefinition is not null)
        {
            throw StateError($"Attribute definition '{def.D}' is a group member and cannot be set independently.");
        }

        // Check value shape vs kind (reusing your shape rules)
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
                if (!pt.IsLookupOptionAllowed(opt, noEntriesMeansAllowAll: true))
                {
                    throw StateError($"Lookup option '{opt.D}' is not allowed for product type '{pt.D}'.");
                }

                break;
            }
            case NumericAttributeDefinition nd:
            {
                if (string.IsNullOrWhiteSpace(Value) ||
                    !decimal.TryParse(Value, out var num))
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

            case StringAttributeDefinition s:
                try
                {
                    s.ValidateValue(Value);
                }
                catch (DomainException ex)
                {
                    throw StateError($"String value '{Value}' is invalid for attribute definition '{def.D}'", ex);
                }
                break;

            case BoolAttributeDefinition b:
                if (!bool.TryParse(Value, out var boolValue))
                {
                    throw StateError($"Attribute definition '{def.D}' expects a numeric value to be set.");
                }
                break;
            case DateAttributeDefinition d:
                if (!DateOnly.TryParseExact(Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
                {
                    throw StateError($"Attribute definition '{def.D}' expects a date value to be set.");
                }
                break;
            default:
                throw StateError($"Unsupported kind '{def.Kind}' for attribute definition '{def.D}'.");
        }

        InvalidDomainStateException StateError(string message, Exception? inner = null) => inner is null
            ? throw new InvalidDomainStateException(this, message)
            : throw new InvalidDomainStateException(this, message, inner);
    }
}
