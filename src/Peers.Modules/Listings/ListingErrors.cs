using System.Numerics;
using Peers.Core.Domain.Errors;
using static Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Listings;

public static class ListingErrors
{
    /// <summary>
    /// Product type '{0}' must be in 'Published' state.
    /// </summary>
    public static DomainError ProductTypeNotPublished(string slugPath) => new(Titles.CannotApplyOperation, "listing.product-type-not-published", slugPath);
    /// <summary>
    /// Product type '{0}' is not selectable for listings.
    /// </summary>
    public static DomainError ProductTypeNotSelectable(string slugPath) => new(Titles.CannotApplyOperation, "listing.product-type-not-selectable", slugPath);
    /// <summary>
    /// Attribute '{0}' is not defined for product type '{1}'.
    /// </summary>
    public static DomainError AttrNotDefined(string key, string slugPath) => new(Titles.CannotApplyOperation, "listing.attribute-not-defined", key, slugPath);
    /// <summary>
    /// Maximum number of variant axes exceeded. The maximum allowed is {0}.
    /// </summary>
    public static DomainError TooManyVariantAxes(int maxVariantAxes) => new(Titles.CannotApplyOperation, "listing.too-many-variant-axes", maxVariantAxes);
    /// <summary>
    /// Attribute '{0}' requires exactly one value.
    /// </summary>
    public static DomainError NonVariantAttrReqExactlyOneValue(string key) => new(Titles.ValidationFailed, "listing.non-variant-attr-req-exactly-one-value", key);
    /// <summary>
    /// Missing required attribute '{0}'.
    /// </summary>
    public static DomainError AttrRequired(string key) => new(Titles.ValidationFailed, "listing.attribute-required", key);
    /// <summary>
    /// Attribute '{0}' requires at least one option to be selected.
    /// </summary>
    public static DomainError VariantAttrReqAtleastOneOption(string key) => new(Titles.ValidationFailed, "listing.variant-attr-req-atleast-one-option", key);
    /// <summary>
    /// Unknown option '{1}' for attribute '{0}'.
    /// </summary>
    public static DomainError UnknownAttrOption(string attrKey, string optionKey) => new(Titles.ValidationFailed, "listing.unknown-attr-option", attrKey, optionKey);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be an integer.
    /// Value '{1}' for attribute '{0}' must be a decimal number.
    /// </summary>
    /// <returns></returns>
    public static DomainError AttrValueMustBeNumeric<T>(string attrKey, string value) where T : struct, INumber<T>
        => new(Titles.ValidationFailed, typeof(T) == typeof(int) ? "listing.attribute-value-must-be-int" : "listing.attribute-value-must-be-decimal", attrKey, value);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must match the pattern '{2}'.
    /// Value '{1}' for attribute '{0}' must not be empty.
    /// </summary>
    public static DomainError AttrValueMustBeValidString(string attrKey, string value, string? regex)
        => regex is not null
            ? new(Titles.ValidationFailed, "listing.attribute-value-must-be-valid-string", attrKey, value, regex)
            : new(Titles.ValidationFailed, "listing.attribute-value-must-be-non-empty-string", attrKey, value);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be either 'true' or 'false'.
    /// </summary>
    public static DomainError AttrValueMustBeBool(string attrKey, string value) => new(Titles.ValidationFailed, "listing.attribute-value-must-be-bool", attrKey, value);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be a valid date in ISO 8601 format (YYYY-MM-DD).
    /// </summary>
    public static DomainError AttrValueMustBeDate(string attrKey, string value) => new(Titles.ValidationFailed, "listing.attribute-value-must-be-date", attrKey, value);
    /// <summary>
    /// Value '{1}' of attribute '{0}' is not allowed for product type '{2}'.
    /// </summary>
    public static DomainError LookupValueNotAllowedForProductType(string attrKey, string value, string slugPath)
        => new(Titles.ValidationFailed, "listing.lookup-value-not-allowed-for-product-type", attrKey, value, slugPath);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be at least '{2}'.
    /// </summary>
    public static DomainError AttrValueMustBeAtLeast<T>(string attrKey, T value, INumber<T> minValue) where T : struct, INumber<T>
        => new(Titles.ValidationFailed, "listing.attribute-value-must-be-at-least", attrKey, value, minValue);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be at most '{2}'.
    /// </summary>
    public static DomainError AttrValueMustBeAtMost<T>(string attrKey, T value, INumber<T> maxValue) where T : struct, INumber<T>
        => new(Titles.ValidationFailed, "listing.attribute-value-must-be-at-most", attrKey, value, maxValue);
    /// <summary>
    /// Minimum order quantity must be at least one.
    /// </summary>
    public static DomainError MinOrderQtyMustBeAtLeastOne => new(Titles.ValidationFailed, "listing.min-order-qty-must-be-at-least-one");
    /// <summary>
    /// Maximum order quantity must be at least one.
    /// </summary>
    public static DomainError MaxOrderQtyMustBeAtLeastOne => new(Titles.ValidationFailed, "listing.max-order-qty-must-be-at-least-one");
    /// <summary>
    /// Maximum order quantity must be greater than or equal to minimum order quantity.
    /// </summary>
    public static DomainError MaxOrderQtyMustBeGreaterThanOrEqualToMin => new(Titles.ValidationFailed, "listing.max-order-qty-must-be-greater-than-or-equal-to-min");
    /// <summary>
    /// The variant with SKU '{0}' could not be found.
    /// </summary>
    public static DomainError VariantNotFound(string sku) => new(Titles.NotFound, "listing.variant-not-found", sku);
    /// <summary>
    /// The variant with SKU '{0}' is inactive.
    /// </summary>
    public static DomainError VariantInactive(string sku) => new(Titles.ValidationFailed, "listing.variant-inactive", sku);
}
