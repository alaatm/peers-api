using NetTopologySuite.Geometries;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Represents the fulfillment preferences for a listing.
/// </summary>
/// <param name="Method">
/// The fulfillment method for orders of this listing. For non-physical products this must be
/// <see cref="FulfillmentMethod.None"/>; for physical products it must be a concrete method.
/// When <see cref="FulfillmentMethod.SellerManaged"/>, the seller uses their own logistics,
/// must provide <paramref name="SellerRate"/>, and the buyer pays shipping.
/// </param>
/// <param name="OutboundPaidBy">
/// Who pays outbound shipping costs. Required for physical products; not allowed for non-physical products.
/// Must be <see cref="ShippingCostPayer.Buyer"/> when <see cref="Method"/> is <see cref="FulfillmentMethod.SellerManaged"/>.
/// If set to <see cref="ShippingCostPayer.Seller"/>, <paramref name="SellerRate"/> must be omitted.
/// </param>
/// <param name="ReturnPaidBy">
/// Who pays return shipping costs. Required when the product is returnable; must be omitted when the product is non-returnable.
/// </param>
/// <param name="NonReturnable">
/// Whether the product is non-returnable. Required for physical products; must be omitted for non-physical products.
/// </param>
/// <param name="OriginLocation">
/// The ship-from (dispatch) location for the product (e.g., seller store or warehouse).
/// Required for physical products; not allowed for non-physical products.
/// </param>
/// <param name="FreeShippingPolicy">
/// A declarative free-shipping policy for the listing, applicable to both platform-managed and seller-managed shipping.
/// The policy is evaluated at checkout time; if it evaluates to “free”, the seller effectively covers shipping costs,
/// regardless of the <paramref name="OutboundPaidBy"/> setting.
/// Allowed for seller-managed rates <b>except</b> when the seller rate kind is <see cref="SellerManagedRateKind.Quote"/>.
/// For quote-based seller-managed shipping, a manual quote is required and policy does not apply.
/// </param>
/// <param name="SellerRate">
/// Seller-managed shipping rate details. Required when <paramref name="Method"/> is <see cref="FulfillmentMethod.SellerManaged"/> and
/// <paramref name="OutboundPaidBy"/> is <see cref="ShippingCostPayer.Buyer"/>.
/// Must be omitted when <paramref name="Method"/> is <see cref="FulfillmentMethod.PlatformManaged"/> or when
/// <paramref name="OutboundPaidBy"/> is <see cref="ShippingCostPayer.Seller"/>.
/// </param>
/// <param name="OrderQtyPolicy">
/// Order-quantity policy (minimum/maximum) for shippable products. Not allowed for non-physical products.
/// </param>
/// <param name="ServiceArea">
/// Geographic service area where fulfillment is available. May apply to both physical and non-physical products.
/// </param>
public sealed record FulfillmentPreferences(
    FulfillmentMethod Method,
    ShippingCostPayer? OutboundPaidBy,
    ShippingCostPayer? ReturnPaidBy,
    bool? NonReturnable,
    Point? OriginLocation,
    FreeShippingPolicy? FreeShippingPolicy,
    SellerManagedRate? SellerRate,
    OrderQtyPolicy? OrderQtyPolicy,
    ServiceArea? ServiceArea)
{
    private FulfillmentPreferences() : this(default!, default!, default!, default!, default!, default!, default!, default!, default!) { }

    /// <summary>
    /// Creates a new instance with default fulfillment preferences.
    /// </summary>
    /// <param name="originLocation">The ship-from (dispatch) location. Required for listings that require shipping (physical products).</param>
    internal static FulfillmentPreferences Default(Point? originLocation) => new()
    {
        OriginLocation = originLocation,
    };

    /// <summary>
    /// Validates the current object's state against the specified product type.
    /// </summary>
    /// <param name="productTypeKind">The product type used to determine validation rules (e.g., physical vs. non-physical).</param>
    internal void Validate(ProductTypeKind productTypeKind)
    {
        if (productTypeKind is ProductTypeKind.Physical)
        {
            if (Method is FulfillmentMethod.None)
            {
                throw new DomainException(E.Logistics.MethodRequiredForPhysicalProducts);
            }
            if (OutboundPaidBy is null)
            {
                throw new DomainException(E.Logistics.ShippingPayerRequired);
            }
            if (NonReturnable is null)
            {
                throw new DomainException(E.Logistics.NonReturnableRequired);
            }
            if (NonReturnable is false && ReturnPaidBy is null)
            {
                throw new DomainException(E.Logistics.ReturnPayerRequiredWhenReturnable);
            }
            if (NonReturnable is true && ReturnPaidBy is not null)
            {
                throw new DomainException(E.Logistics.ReturnPayerNotAllowedWhenNonReturnable);
            }
            if (OriginLocation is null)
            {
                throw new DomainException(E.Logistics.OriginLocationRequired);
            }
            if (Method is FulfillmentMethod.PlatformManaged &&
                SellerRate is not null)
            {
                throw new DomainException(E.Logistics.SellerRateNotAllowedForPlatformManagedShipping);
            }
            if (Method is FulfillmentMethod.SellerManaged)
            {
                if (OutboundPaidBy is not ShippingCostPayer.Buyer)
                {
                    throw new DomainException(E.Logistics.SellerManagedRequiresBuyerAsPayer);
                }

                if (SellerRate is null)
                {
                    throw new DomainException(E.Logistics.SellerRateRequired);
                }

                // Disallow FreeShippingPolicy with quote-based seller-managed rates.
                if (SellerRate.Kind is SellerManagedRateKind.Quote &&
                    FreeShippingPolicy is not null)
                {
                    throw new DomainException(E.Logistics.FreeShippingPolicyNotAllowedForQuoteBasedShipping);
                }
            }

            // If ever seller pays (for some other method), a SellerRate makes no sense
            if (OutboundPaidBy is ShippingCostPayer.Seller && SellerRate is not null)
            {
                throw new DomainException(E.Logistics.SellerRateNotAllowedWhenSellerPays);
            }
        }
        else
        {
            if (Method is not FulfillmentMethod.None)
            {
                throw new DomainException(E.Logistics.MethodMustBeNoneForNonPhysicalProducts);
            }
            if (OutboundPaidBy is not null)
            {
                throw new DomainException(E.Logistics.ShippingPayerNotAllowedForNonPhysicalProducts);
            }
            if (ReturnPaidBy is not null)
            {
                throw new DomainException(E.Logistics.ReturnPayerNotAllowedForNonPhysicalProducts);
            }
            if (NonReturnable is not null)
            {
                throw new DomainException(E.Logistics.NonReturnableNotAllowedForNonPhysicalProducts);
            }
            if (OriginLocation is not null)
            {
                throw new DomainException(E.Logistics.OriginLocationNotAllowedForNonPhysicalProducts);
            }
            if (FreeShippingPolicy is not null)
            {
                throw new DomainException(E.Logistics.FreeShippingPolicyNotAllowedForNonPhysicalProducts);
            }
            if (SellerRate is not null)
            {
                throw new DomainException(E.Logistics.SellerRateNotAllowedForNonPhysicalProducts);
            }
            if (OrderQtyPolicy is not null)
            {
                throw new DomainException(E.Logistics.OrderQtyNotAllowedForNonPhysicalProducts);
            }
            if (productTypeKind is ProductTypeKind.Service &&
                ServiceArea is null)
            {
                throw new DomainException(E.Logistics.ServiceAreaRequiredForServices);
            }
            if (productTypeKind is ProductTypeKind.Digital &&
                ServiceArea is not null)
            {
                throw new DomainException(E.Logistics.ServiceAreaNotAllowedForDigitalProducts);
            }
        }

        FreeShippingPolicy?.Validate();
        SellerRate?.Validate();
        OrderQtyPolicy?.Validate();
        ServiceArea?.Validate();
    }
}
