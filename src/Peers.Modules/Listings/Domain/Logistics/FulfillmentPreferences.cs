using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Sellers.Domain;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Represents the fulfillment preferences for a listing.
/// </summary>
/// <param name="Method">
/// The fulfillment method for orders of this listing. For non-physical products this must be
/// <see cref="FulfillmentMethod.None"/>; for physical products it must be a concrete method.
/// </param>
/// <param name="OutboundPaidBy">
/// Who pays outbound shipping costs. Required for physical products; not allowed for non-physical products.
/// Must be <see cref="ShippingCostPayer.Buyer"/> when <see cref="Method"/> is <see cref="FulfillmentMethod.SellerManaged"/>.
/// </param>
/// <param name="ReturnPaidBy">
/// Who pays return shipping costs. Required when the product is returnable; must be omitted when the product is non-returnable.
/// </param>
/// <param name="NonReturnable">
/// Whether the product is non-returnable. Required for physical products; must be omitted for non-physical products.
/// </param>
/// <param name="OrderQtyPolicy">
/// Order-quantity policy (minimum/maximum) for shippable products. Not allowed for non-physical products.
/// </param>
/// <param name="ServiceArea">
/// Geographic service area where fulfillment is available. May apply to both physical and non-physical (services) products.
/// </param>
public sealed record FulfillmentPreferences(
    FulfillmentMethod Method,
    ShippingCostPayer? OutboundPaidBy,
    ShippingCostPayer? ReturnPaidBy,
    bool? NonReturnable,
    OrderQtyPolicy? OrderQtyPolicy,
    ServiceArea? ServiceArea)
{
    private FulfillmentPreferences() : this(default!, default!, default!, default!, default!, default!) { }

    /// <summary>
    /// Validates the current object's state against the specified product type.
    /// </summary>
    /// <param name="productTypeKind">The product type used to determine validation rules (e.g., physical vs. non-physical).</param>
    /// <param name="shippingProfile">The selected shipping profile to ensure compatibility.</param>
    internal void Validate(
        ProductTypeKind productTypeKind,
        ShippingProfile? shippingProfile)
    {
        if (productTypeKind is ProductTypeKind.Physical)
        {
            var expectedMethod = shippingProfile is null
                ? FulfillmentMethod.PlatformManaged
                : FulfillmentMethod.SellerManaged;

            if (Method != expectedMethod)
            {
                throw new DomainException(E.Logistics.IncompatibleMethodForPhysicalProduct(expectedMethod, Method));
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
            if (Method is FulfillmentMethod.SellerManaged &&
                OutboundPaidBy is not ShippingCostPayer.Buyer)
            {
                throw new DomainException(E.Logistics.SellerManagedRequiresBuyerAsPayer);
            }
        }
        else
        {
            if (Method is not FulfillmentMethod.None)
            {
                throw new DomainException(E.Logistics.MethodMustBeNoneForNonPhysicalProducts);
            }

            if (shippingProfile is not null)
            {
                throw new DomainException(E.Logistics.ShippingProfileNotAllowedForNonPhysicalProducts);
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
            if (OrderQtyPolicy is not null)
            {
                throw new DomainException(E.Logistics.OrderQtyNotAllowedForNonPhysicalProducts);
            }

            if (productTypeKind is ProductTypeKind.Service)
            {
                if (ServiceArea is null)
                {
                    throw new DomainException(E.Logistics.ServiceAreaRequiredForServices);
                }
            }
            else // Digital or other non-physical, non-service types
            {
                if (ServiceArea is not null)
                {
                    throw new DomainException(E.Logistics.ServiceAreaNotAllowedForDigitalProducts);
                }
            }
        }

        OrderQtyPolicy?.Validate();
        ServiceArea?.Validate();
    }
}
