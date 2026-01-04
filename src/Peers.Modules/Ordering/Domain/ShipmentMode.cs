using System.Diagnostics;
using Peers.Modules.Listings.Domain.Logistics;

namespace Peers.Modules.Ordering.Domain;

/// <summary>
/// Specifies the shipment mode for an order.
/// </summary>
public enum ShipmentMode
{
    /// <summary>
    /// Shipping and fulfillment are managed by the platform using integrated carriers and logistics services.
    /// </summary>
    PlatformManaged,
    /// <summary>
    /// Shipping and fulfillment are managed by the seller using their own carriers and logistics processes.
    /// </summary>
    SellerManaged,
}

/// <summary>
/// Provides extension methods for the FulfillmentMethod enumeration
/// </summary>
public static class FulfillmentMethodExtensions
{
    extension(FulfillmentMethod value)
    {
        /// <summary>
        /// Converts the current fulfillment method value to its corresponding shipment mode.
        /// </summary>
        /// <returns>The shipment mode that corresponds to the current fulfillment method.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the fulfillment method is 'None', which cannot be converted to a shipment mode.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the fulfillment method value is not recognized.</exception>
        public ShipmentMode ToShippingMode() => value switch
        {
            FulfillmentMethod.PlatformManaged => ShipmentMode.PlatformManaged,
            FulfillmentMethod.SellerManaged => ShipmentMode.SellerManaged,
            FulfillmentMethod.None => throw new InvalidOperationException("Cannot convert 'None' fulfillment method to shipment mode."),
            _ => throw new UnreachableException(),
        };
    }
}
