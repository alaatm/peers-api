using Peers.Core.Domain.Errors;
using static Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Sellers;

public static class SellersErrors
{
    /// <summary>
    /// A shipping profile with the name '{0}' already exists.
    /// </summary>
    public static DomainError DuplicateShippingProfileName(string name) => new(Titles.ValidationFailed, "sellers.duplicate_shipping_profile_name", name);
    /// <summary>
    /// Free-shipping policy is not allowed with quote-based seller-managed shipping.
    /// </summary>
    public static DomainError FreeShippingPolicyNotAllowedForQuoteBasedShipping => new(Titles.ValidationFailed, "sellers.free_shipping_policy_not_allowed_for_quote_based_shipping");
    /// <summary>
    /// Seller shipping rate configuration is required.
    /// </summary>
    public static DomainError SellerRateRequired => new(Titles.ValidationFailed, "sellers.seller-rate-required");
    /// <summary>
    /// Shipping location is required.
    /// </summary>
    public static DomainError OriginLocationRequired => new(Titles.ValidationFailed, "sellers.shipping-location-required");
}
