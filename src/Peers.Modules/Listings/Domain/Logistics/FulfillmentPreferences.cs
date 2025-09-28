using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Represents the fulfillment preferences for a listing, including fulfillment method, payer, pay-on-delivery,
/// whether the listing is non-returnable, order quantity policy, and service area.
/// </summary>
/// <param name="Method">Indicates the method of fulfillment for orders of this listing.</param>
/// <param name="ShippingPayer">Indicates who is responsible for paying the shipping costs.</param>
/// <param name="ReturnPayer">Indicates who is responsible for paying the return shipping costs.</param>
/// <param name="AllowPayOnDelivery">Indicates whether pay-on-delivery is allowed for this listing.</param>
/// <param name="NonReturnable">Indicates whether the listing is non-returnable.</param>
/// <param name="OrderQty">The order quantity policy for the listing, defining minimum and maximum order quantities, if required.</param>
/// <param name="ServiceArea">The geographic service area where fulfillment is available, if required.</param>
public sealed record FulfillmentPreferences(
    FulfillmentMethod Method,
    ShippingPayer ShippingPayer,
    ShippingPayer ReturnPayer,
    bool AllowPayOnDelivery,
    bool NonReturnable,
    OrderQtyPolicy? OrderQty,
    ServiceArea? ServiceArea)
{
    private FulfillmentPreferences() : this(default!, default!, default!, default!, default!, default!, default!) { }

    /// <summary>
    /// Creates a new instance of the default fulfillment preferences.
    /// </summary>
    internal static FulfillmentPreferences Default() => new();

    /// <summary>
    /// Validates the current object's state.
    /// </summary>
    /// <param name="productTypeKind">The kind of product type, used to determine specific validation rules.</param>
    internal void Validate(ProductTypeKind productTypeKind)
    {
        if (productTypeKind is ProductTypeKind.Physical)
        {
            if (ShippingPayer is ShippingPayer.None)
            {
                throw new DomainException(E.Logistics.ShippingPayerRequired);
            }
            if (!NonReturnable && ReturnPayer is ShippingPayer.None)
            {
                throw new DomainException(E.Logistics.ReturnPayerRequired);
            }
        }

        OrderQty?.Validate();
        ServiceArea?.Validate();
    }
}
