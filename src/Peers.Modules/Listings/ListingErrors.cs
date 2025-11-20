using System.Numerics;
using Peers.Core.Domain.Errors;
using Peers.Core.Localization;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Logistics;
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
    /// Shipping profile can only be applied to listings for physical product types.
    /// </summary>
    public static DomainError ShippingProfileOnlyForPhysicalProductTypes => new(Titles.CannotApplyOperation, "listing.shipping-profile-only-for-physical-product-types");
    /// <summary>
    /// A default shipping address is required to create a listing for a product that requires physical shipping or delivery.
    /// </summary>
    public static DomainError SellerMustHaveAddress => new(Titles.CannotApplyOperation, "listing.seller-must-have-address");
    /// <summary>
    /// Attribute '{0}' is not defined for product type '{1}'.
    /// </summary>
    public static DomainError AttrNotDefined(string key, string slugPath) => new(Titles.CannotApplyOperation, "listing.attribute-not-defined", key, slugPath);
    /// <summary>
    /// Attribute '{0}' value cannot be null.
    /// </summary>
    public static DomainError AttrValueCannotBeNull(string key) => new(Titles.ValidationFailed, "listing.attribute-value-cannot-be-null", key);
    /// <summary>
    /// Maximum number of variant axes '{0}' exceeded. The provided number of axes is {1}.
    /// </summary>
    public static DomainError VariantAxesCapExceeded(int variantAxesCap, int providedAxesCount) => new(Titles.CannotApplyOperation, "listing.variant-axes-cap-exceeded", variantAxesCap, providedAxesCount);
    /// <summary>
    /// Maximum number of SKUs '{0}' exceeded. The computed number of SKUs is {1}.
    /// </summary>
    public static DomainError SkuCapExceeded(int skusCap, long computedSkus) => new(Titles.CannotApplyOperation, "listing.sku-cap-exceeded", skusCap, computedSkus);
    /// <summary>
    /// Attribute '{0}' requires exactly one value.
    /// </summary>
    public static DomainError NonVariantAttrReqExactlyOneValue(string key) => new(Titles.ValidationFailed, "listing.non-variant-attr-req-exactly-one-value", key);
    /// <summary>
    /// Missing required attribute '{0}'.
    /// </summary>
    public static DomainError AttrReq(string key) => new(Titles.ValidationFailed, "listing.attribute-required", key);
    /// <summary>
    /// Attribute '{0}' requires at least one option to be selected.
    /// </summary>
    public static DomainError VariantAttrReqAtleastOneOption(string key) => new(Titles.ValidationFailed, "listing.variant-attr-req-atleast-one-option", key);
    /// <summary>
    /// Member attribute '{0}' of group '{1}' cannot be set directly. Set the group attribute instead.
    /// </summary>
    public static DomainError GroupMemberCannotBeHeader(string memberKey, string groupKey) => new(Titles.ValidationFailed, "listing.group-member-cannot-be-header", memberKey, groupKey);
    /// <summary>
    /// Non-variant attribute '{0}' cannot be used as a variant axis.
    /// </summary>
    public static DomainError NonVariantAttrDoesNotAcceptAxis(string key) => new(Titles.ValidationFailed, "listing.non-variant-attr-does-not-accept-axis", key);
    /// <summary>
    /// Unknown option '{1}' for enum attribute '{0}'.
    /// </summary>
    public static DomainError UnknownEnumAttrOpt(string attrKey, string optCode) => new(Titles.ValidationFailed, "listing.unknown-enum-attr-opt", attrKey, optCode);
    /// <summary>
    /// Unknown option '{1}' for lookup attribute '{0}'.
    /// </summary>
    public static DomainError UnknownLookupAttrOpt(string attrKey, string optCode) => new(Titles.ValidationFailed, "listing.unknown-lookup-attr-opt", attrKey, optCode);
    /// <summary>
    /// Attribute axis '{0}' requires one or more enum option codes.
    /// </summary>
    public static DomainError AxisReqEnumOptAxis(string attrKey) => new(Titles.ValidationFailed, "listing.axis-req-enum-opt-axis", attrKey);
    /// <summary>
    /// Attribute axis '{0}' requires one or more lookup option codes.
    /// </summary>
    public static DomainError AxisReqLookupOptAxis(string attrKey) => new(Titles.ValidationFailed, "listing.axis-req-lookup-opt-axis", attrKey);
    /// <summary>
    /// Attribute axis '{0}' requires one or more numeric values.
    /// </summary>
    public static DomainError AxisReqNumericAxis(string attrKey) => new(Titles.ValidationFailed, "listing.axis-req-numeric-axis", attrKey);
    /// <summary>
    /// Group axis '{0}' contains one or more duplicate values.
    /// </summary>
    /// <param name="attrKey"></param>
    /// <returns></returns>
    public static DomainError DuplicateGroupAxisValue(string attrKey) => new(Titles.ValidationFailed, "listing.duplicate-group-axis-value", attrKey);
    /// <summary>
    /// Attribute axis '{0}' requires a matrix of numeric values.
    /// </summary>
    public static DomainError AxisReqMatrix(string attrKey) => new(Titles.ValidationFailed, "listing.axis-req-matrix", attrKey);
    /// <summary>
    /// Attribute '{0}' of kind '{1}' cannot be used as a variant axis.
    /// </summary>
    public static DomainError UnsupportedVariantInput(string attrKey, AttributeKind kind) => new(Titles.ValidationFailed, "listing.unsupported-variant-input", attrKey, kind.ToString().ToLowerInvariant());
    /// <summary>
    /// Attribute axis '{0}' requires at least {1} value(s).
    /// </summary>
    public static DomainError AxisReqAtLeastMinValues(string attrKey, int min) => new(Titles.ValidationFailed, "listing.axis-req-at-least-min-values", attrKey, min);
    /// <summary>
    /// Attribute axis '{0}' requires exactly {1} value(s).
    /// </summary>
    public static DomainError AxisReqExactlyNValues(string attrKey, int exact) => new(Titles.ValidationFailed, "listing.axis-req-exactly-n-values", attrKey, exact);
    /// <summary>
    /// Attribute axis '{0}' must not have duplicate values. Duplicate value: '{1}'.
    /// </summary>
    public static DomainError AxisMustNotHaveDuplicateValues(string attrKey, string dup) => new(Titles.ValidationFailed, "listing.axis-must-not-have-duplicate-values", attrKey, dup);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be an integer.
    /// Value '{1}' for attribute '{0}' must be a decimal number.
    /// </summary>
    /// <returns></returns>
    public static DomainError AttrValueMustBeNumeric<T>(string attrKey, string value) where T : struct, INumber<T>
        => new(Titles.ValidationFailed, typeof(T) == typeof(int) ? "listing.attribute-value-must-be-int" : "listing.attribute-value-must-be-decimal", attrKey, value);
    /// <summary>
    /// Attribute '{0}' requires exactly one numeric value.
    /// </summary>
    public static DomainError AttrReqSingleNumValue(string attrKey) => new(Titles.ValidationFailed, "listing.invalid-numeric-attribute-value", attrKey);
    /// <summary>
    /// Attribute '{0}' requires exactly one string value.
    /// </summary>
    public static DomainError AttrReqSingleStrValue(string attrKey) => new(Titles.ValidationFailed, "listing.invalid-string-attribute-value", attrKey);
    /// <summary>
    /// Attribute '{0}' requires exactly one boolean value.
    /// </summary>
    public static DomainError AttrReqSingleBoolValue(string attrKey) => new(Titles.ValidationFailed, "listing.invalid-bool-attribute-value", attrKey);
    /// <summary>
    /// Attribute '{0}' requires exactly one date value.
    /// </summary>
    public static DomainError AttrReqSingleDateValue(string attrKey) => new(Titles.ValidationFailed, "listing.invalid-date-attribute-value", attrKey);
    /// <summary>
    /// Attribute '{0}' requires exactly one enum option code.
    /// </summary>
    public static DomainError AttrReqSingleEnumOptCodeValue(string attrKey) => new(Titles.ValidationFailed, "listing.invalid-enum-attribute-value", attrKey);
    /// <summary>
    /// Attribute '{0}' requires exactly one lookup option code.
    /// </summary>
    public static DomainError AttrReqSingleLookupOptCodeValue(string attrKey) => new(Titles.ValidationFailed, "listing.invalid-lookup-attribute-value", attrKey);
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
    /// Value '{1}' of attribute '{0}' is not allowed.
    /// </summary>
    public static DomainError LookupOptNotAllowedByAttr(string attrKey, string value)
        => new(Titles.ValidationFailed, "listing.lookup-opt-not-allowed-by-attr", attrKey, value);
    /// <summary>
    /// Enum option '{1}' for attribute '{0}' is not reachable from parent attributes.
    /// </summary>
    public static DomainError EnumOptNotReachableFromParents(string attrKey, string optCode)
        => new(Titles.ValidationFailed, "listing.enum-opt-not-reachable-from-parents", attrKey, optCode);
    /// <summary>
    /// Lookup option '{1}' for attribute '{0}' is not reachable from parent attributes.
    /// </summary>
    public static DomainError LookupOptNotReachableFromParents(string attrKey, string optCode)
        => new(Titles.ValidationFailed, "listing.lookup-opt-not-reachable-from-parents", attrKey, optCode);
    /// <summary>
    /// Appending variants is only allowed after the listing is published.
    /// </summary>
    public static DomainError AppendOnlyPostPublish => new(Titles.CannotApplyOperation, "listing.append-only-post-publish");
    /// <summary>
    /// The operation requires the listing snapshot ID to match.
    /// </summary>
    public static DomainError SnapshotMismatch => new(Titles.CannotApplyOperation, "listing.snapshot-mismatch");
    /// <summary>
    /// Appending variants requires at least one variant axis to be defined.
    /// </summary>
    public static DomainError AppendRequiresAtLeastOneVariantAxis => new(Titles.CannotApplyOperation, "listing.append-requires-at-least-one-variant-axis");
    /// <summary>
    /// Cannot add new variant axis '{0}' after the listing is published.
    /// </summary>
    public static DomainError CannotAddNewAxisPostPublish(string key) => new(Titles.CannotApplyOperation, "listing.cannot-add-new-axis-post-publish", key);
    /// <summary>
    /// Appending variants requires at least one new variant value.
    /// </summary>
    public static DomainError AppendRequiresAtLeastOneNewVariantValue => new(Titles.CannotApplyOperation, "listing.append-requires-at-least-one-new-variant-value");
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
        /// Fulfillment method '{1}' is incompatible for physical products. Expected method: '{0}'.
        /// </summary>
        public static DomainError IncompatibleMethodForPhysicalProduct(FulfillmentMethod expected, FulfillmentMethod actual)
            => new(Titles.ValidationFailed, "listing.logistics.incompatible-method-for-physical-product", expected.ToString(), actual.ToString());
        /// <summary>
        /// Fulfillment method must be 'None' for non-physical products.
        /// </summary>
        public static DomainError MethodMustBeNoneForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.method-must-be-none-for-nonphysical");
        /// <summary>
        /// Shipping profile is not allowed for non-physical products.
        /// </summary>
        public static DomainError ShippingProfileNotAllowedForNonPhysicalProducts => new(Titles.ValidationFailed, "listing.logistics.shipping-profile-not-allowed-for-nonphysical");
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
