using NetTopologySuite.Geometries;

namespace Peers.Core.Shipping;

/// <summary>
/// Represents a service for calculating shipping costs for platform-managed shipments.
/// </summary>
public interface IPlatformShippingService
{
    /// <summary>
    /// Computes the shipping cost for a platform-managed shipment.
    /// </summary>
    /// <param name="deliveryLocation">The delivery location as a geographic point.</param>
    /// <param name="items">The items to be shipped.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns>The computed shipping cost.</returns>
    Task<decimal> ComputeAsync(
        Point deliveryLocation,
        PlatformShipmentItem[] items,
        CancellationToken ctk);
}
