using System.Numerics;
using Peers.Core.Domain.Errors;
using Peers.Core.Localization;
using static Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Listings;

public static class ListingErrors
{
    /// <summary>
    /// Listing must be in 'Draft' state.
    /// </summary>
    public static DomainError NotDraft => new(Titles.CannotApplyOperation, "listing.not-draft");
    /// <summary>
    /// Listing is already in 'Published' state.
    /// </summary>
    public static DomainError AlreadyPublished => new(Titles.CannotApplyOperation, "listing.already-published");
    /// <summary>
    /// Product type '{0}' must be in 'Published' state.
    /// </summary>
    public static DomainError ProductTypeNotPublished(string slugPath) => new(Titles.CannotApplyOperation, "listing.product-type-not-published", slugPath);
    /// <summary>
    /// Product type '{0}' is not selectable for listings.
    /// </summary>
    public static DomainError ProductTypeNotSelectable(string slugPath) => new(Titles.CannotApplyOperation, "listing.product-type-not-selectable", slugPath);
    /// <summary>
    /// A default shipping address is required to create a listing for a product that requires physical shipping or delivery.
    /// </summary>
    public static DomainError SellerMustHaveAddress => new(Titles.CannotApplyOperation, "listing.seller-must-have-address");
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
    /// The variant with SKU '{0}' could not be found.
    /// </summary>
    public static DomainError VariantNotFound(string sku) => new(Titles.NotFound, "listing.variant-not-found", sku);
    /// <summary>
    /// The variant with SKU '{0}' is inactive.
    /// </summary>
    public static DomainError VariantInactive(string sku) => new(Titles.ValidationFailed, "listing.variant-inactive", sku);
    /// <summary>
    /// Logistics information is required for platform-managed listings.
    /// </summary>
    public static DomainError LogisticsRequiredForPlatformManaged => new(Titles.ValidationFailed, "listing.logistics-required-for-platform-managed");
    /// <summary>
    /// Logistics information is not allowed for seller-managed listings.
    /// </summary>
    public static DomainError LogisticsNotAllowedForSellerManaged => new(Titles.ValidationFailed, "listing.logistics-not-allowed-for-seller-managed");
    /// <summary>
    /// Logistics information is not allowed for seller-managed listings but exist on SKU(s) {0}. Clear logistics information from these SKU(s) first.
    /// </summary>
    public static DomainError CannotSetSellerManagedWhenLogisticsExist(string[] skus)
        => new(Titles.ValidationFailed, "listing.logistics-not-allowed-for-seller-managed-with-skus", LocalizationHelper.FormatList(skus));
    /// <summary>
    /// Fulfillment method must be set for listings that require physical shipping or delivery.
    /// </summary>
    public static DomainError FulfillmentMethodMustBeSet => new(Titles.ValidationFailed, "listing.fulfillment-method-must-be-set");
    /// <summary>
    /// Fulfillment method must be 'None' for listings that do not require physical shipping or delivery.
    /// </summary>
    public static DomainError FulfillmentMethodMustBeNone => new(Titles.ValidationFailed, "listing.fulfillment-method-must-be-none");
    /// <summary>
    /// Logistics information can only be set for listings that require physical shipping or delivery.
    /// </summary>
    public static DomainError LogisticsApplyOnlyToPhysicalListings => new(Titles.ValidationFailed, "listing.logistics-apply-only-to-physical-listings");
    /// <summary>
    /// Logistics information is required only for platform-managed listings.
    /// </summary>
    public static DomainError LogisticsRequiredOnlyForPlatformManaged => new(Titles.ValidationFailed, "listing.logistics-required-only-for-platform-managed");
    /// <summary>
    /// At least one variant is required.
    /// </summary>
    public static DomainError AtLeastOneVariantRequired => new(Titles.CannotApplyOperation, "listing.at-least-one-variant-required");
    /// <summary>
    /// Variant must specify a value for variant axis '{0}'.
    /// </summary>
    public static DomainError VariantMissingAxis(string key) => new(Titles.CannotApplyOperation, "listing.variant-missing-axis", key);
    /// <summary>
    /// A variant with the same option combination already exists.
    /// </summary>
    public static DomainError DuplicateVariantCombination => new(Titles.CannotApplyOperation, "listing.duplicate-variant-combination");
    /// <summary>
    /// Exactly one default variant is required when no variant axes are defined.
    /// </summary>
    public static DomainError SingleDefaultVariantExpected => new(Titles.CannotApplyOperation, "listing.single-default-variant-expected");
    /// <summary>
    /// Logistics profile is required for physical products.
    /// </summary>
    public static DomainError LogisticsRequiredForPhysicalProducts => new(Titles.CannotApplyOperation, "listing.logistics-required-for-physical-products");

    public static class Logistics
    {
        /// <summary>
        /// Dimensions must be positive values.
        /// </summary>
        public static DomainError DimensionsMustBePositive => new(Titles.ValidationFailed, "listing.logistics.dimensions-must-be-positive");
        /// <summary>
        /// Weight must be a positive value.
        /// </summary>
        public static DomainError WeightMustBePositive => new(Titles.ValidationFailed, "listing.logistics.weight-must-be-positive");
        /// <summary>
        /// A return payer must be specified if the listing is returnable.
        /// </summary>
        public static DomainError ReturnPayerRequired => new(Titles.ValidationFailed, "listing.logistics.return-payer-required");
        /// <summary>
        /// A shipping payer must be specified if the listing requires physical shipping or delivery.
        /// </summary>
        public static DomainError ShippingPayerRequired => new(Titles.ValidationFailed, "listing.logistics.shipping-payer-required");
        /// <summary>
        /// Minimum order quantity must be at least one.
        /// </summary>
        public static DomainError MinOrderQtyMustBeAtLeastOne => new(Titles.ValidationFailed, "listing.logistics.min-order-qty-must-be-at-least-one");
        /// <summary>
        /// Maximum order quantity must be at least one.
        /// </summary>
        public static DomainError MaxOrderQtyMustBeAtLeastOne => new(Titles.ValidationFailed, "listing.logistics.max-order-qty-must-be-at-least-one");
        /// <summary>
        /// Maximum order quantity must be greater than or equal to minimum order quantity.
        /// </summary>
        public static DomainError MaxOrderQtyMustBeGreaterThanOrEqualToMin => new(Titles.ValidationFailed, "listing.logistics.max-order-qty-must-be-greater-than-or-equal-to-min");
        /// <summary>
        /// Service area center is required.
        /// </summary>
        public static DomainError ServiceAreaCenterRequired => new(Titles.ValidationFailed, "listing.logistics.service-area-center-required");
        /// <summary>
        /// Service area center latitude must be between -90 and 90 degrees.
        /// </summary>
        public static DomainError ServiceAreaCenterInvalidLatitude => new(Titles.ValidationFailed, "listing.logistics.service-area-center-invalid-latitude");
        /// <summary>
        /// Service area center longitude must be between -180 and 180 degrees.
        /// </summary>
        public static DomainError ServiceAreaCenterInvalidLongitude => new(Titles.ValidationFailed, "listing.logistics.service-area-center-invalid-longitude");
        /// <summary>
        /// Service area radius must be a non-negative value.
        /// </summary>
        public static DomainError ServiceAreaRadiusMustBeNonNegative => new(Titles.ValidationFailed, "listing.logistics.service-area-radius-must-be-non-negative");
        /// <summary>
        /// Free shipping policy is not allowed for listings that do not require physical shipping or delivery.
        /// </summary>
        public static DomainError FreeShippingPolicyNotAllowed => new(Titles.ValidationFailed, "listing.logistics.free-shipping-policy-not-allowed");
        /// <summary>
        /// The minimum order amount for free shipping must be a non-negative value.
        /// </summary>
        public static DomainError FreeShippingMinOrderMustBeNonNegative => new(Titles.ValidationFailed, "listing.logistics.free-shipping-min-order-must-be-non-negative");
        /// <summary>
        /// The maximum delivery distance for free shipping must be a non-negative value.
        /// </summary>
        public static DomainError FreeShippingMaxDistanceMetersMustBeNonNegative => new(Titles.ValidationFailed, "listing.logistics.free-shipping-within-meters-must-be-non-negative");
        /// <summary>
        /// Quote-based shipping rates cannot be computed automatically.
        /// </summary>
        public static DomainError CannotComputeQuoteRate => new(Titles.CannotApplyOperation, "listing.logistics.cannot-compute-quote-rate");
        /// <summary>
        /// Flat based rate requires a valid flat amount value.
        /// </summary>
        public static DomainError InvalidFlatRate => new(Titles.ValidationFailed, "listing.logistics.invalid-flat-rate");
        /// <summary>
        /// Weight based rate requires a valid weight rate and base fee values.
        /// </summary>
        public static DomainError InvalidWeightRate => new(Titles.ValidationFailed, "listing.logistics.invalid-weight-rate");
        /// <summary>
        /// Distance based rate requires a valid distance rate and base fee values.
        /// </summary>
        public static DomainError InvalidDistanceRate => new(Titles.ValidationFailed, "listing.logistics.invalid-distance-rate");
        /// <summary>
        /// Invalid seller rate kind.
        /// </summary>
        public static DomainError InvalidSellerRateKind => new(Titles.ValidationFailed, "listing.logistics.invalid-seller-rate-kind");
        /// <summary>
        /// The minimum fee must be a non-negative value.
        /// </summary>
        public static DomainError InvalidMinFee => new(Titles.ValidationFailed, "listing.logistics.invalid-min-fee");
        /// <summary>
        /// Seller shipping rate configuration is required for listings with seller-managed fulfillment method.
        /// </summary>
        public static DomainError SellerRateRequired => new(Titles.ValidationFailed, "listing.logistics.seller-rate-required");
        /// <summary>
        /// Seller shipping rate configuration is not allowed for listings with non seller-managed fulfillment method.
        /// </summary>
        public static DomainError SellerRateNotAllowed => new(Titles.ValidationFailed, "listing.logistics.seller-rate-not-allowed");
        /// <summary>
        /// A shipping location is required for listings that require physical shipping or delivery.
        /// </summary>
        public static DomainError OriginLocationRequired => new(Titles.ValidationFailed, "listing.logistics.shipping-location-required");
        /// <summary>
        /// Fulfillment method is required for physical products.
        /// </summary>
        public static DomainError MethodRequiredForPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.method-required-for-physical-products");
        /// <summary>
        /// Non-returnable flag must be specified for physical products.
        /// </summary>
        public static DomainError NonReturnableRequired => new(Titles.ValidationFailed, "listing.logistics.nonreturnable-required");
        /// <summary>
        /// Return shipping payer must be specified when the product is returnable.
        /// </summary>
        public static DomainError ReturnPayerRequiredWhenReturnable => new(Titles.ValidationFailed, "listing.logistics.return-payer-required-when-returnable");
        /// <summary>
        /// Return shipping payer is not allowed when the product is non-returnable.
        /// </summary>
        public static DomainError ReturnPayerNotAllowedWhenNonReturnable => new(Titles.ValidationFailed, "listing.logistics.return-payer-not-allowed-when-nonreturnable");
        /// <summary>
        /// Seller-managed rate is not allowed for platform-managed fulfillment.
        /// </summary>
        public static DomainError SellerRateNotAllowedForPlatformManagedShipping => new(Titles.ValidationFailed, "listing.logistics.seller-rate-not-allowed-for-platform-managed");
        /// <summary>
        /// Seller-managed fulfillment requires the buyer to pay shipping.
        /// </summary>
        public static DomainError SellerManagedRequiresBuyerAsPayer => new(Titles.ValidationFailed, "listing.logistics.seller-managed-requires-buyer-payer");
        /// <summary>
        /// Free-shipping policy is not allowed with quote-based seller-managed shipping.
        /// </summary>
        public static DomainError FreeShippingPolicyNotAllowedForQuoteBasedShipping => new(Titles.ValidationFailed, "listing.logistics.free-shipping-policy-not-allowed-for-quote");
        /// <summary>
        /// Seller-managed rate must not be provided when the seller pays shipping.
        /// </summary>
        public static DomainError SellerRateNotAllowedWhenSellerPays => new(Titles.ValidationFailed, "listing.logistics.seller-rate-not-allowed-when-seller-pays");
        /// <summary>
        /// Fulfillment method must be 'None' for non-physical products.
        /// </summary>
        public static DomainError MethodMustBeNoneForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.method-must-be-none-for-nonphysical");
        /// <summary>
        /// Shipping payer is not allowed for non-physical products.
        /// </summary>
        public static DomainError ShippingPayerNotAllowedForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.shipping-payer-not-allowed-for-nonphysical");
        /// <summary>
        /// Return shipping payer is not allowed for non-physical products.
        /// </summary>
        public static DomainError ReturnPayerNotAllowedForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.return-payer-not-allowed-for-nonphysical");
        /// <summary>
        /// Non-returnable flag is not allowed for non-physical products.
        /// </summary>
        public static DomainError NonReturnableNotAllowedForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.nonreturnable-not-allowed-for-nonphysical");
        /// <summary>
        /// Origin location is not allowed for non-physical products.
        /// </summary>
        public static DomainError OriginLocationNotAllowedForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.origin-location-not-allowed-for-nonphysical");
        /// <summary>
        /// Free-shipping policy is not allowed for non-physical products.
        /// </summary>
        public static DomainError FreeShippingPolicyNotAllowedForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.free-shipping-policy-not-allowed-for-nonphysical");
        /// <summary>
        /// Seller-managed rate is not allowed for non-physical products.
        /// </summary>
        public static DomainError SellerRateNotAllowedForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.seller-rate-not-allowed-for-nonphysical");
        /// <summary>
        /// Order quantity policy is not allowed for non-physical products.
        /// </summary>
        public static DomainError OrderQtyNotAllowedForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.order-qty-not-allowed-for-nonphysical");
        /// <summary>
        /// Service area is required for service products.
        /// </summary>
        public static DomainError ServiceAreaRequiredForServices => new(Titles.ValidationFailed, "listing.logistics.service-area-required-for-services");
        /// <summary>
        /// Service area is not allowed for digital products.
        /// </summary>
        public static DomainError ServiceAreaNotAllowedForDigitalProducts => new(Titles.ValidationFailed, "listing.logistics.service-area-not-allowed-for-digital");
        /// <summary>
        /// Quote-based seller-managed rate must not specify pricing fields.
        /// </summary>
        public static DomainError QuoteRateMustNotSpecifyPricingFields => new(Titles.ValidationFailed, "listing.logistics.quote-rate-must-not-specify-pricing-fields");
    }
}
