using NetTopologySuite.Geometries;
using Peers.Modules.Carts.Domain;

namespace Peers.Modules.Carts.Services;

/// <summary>
/// Represents a service that calculates shipping costs for a given cart and delivery location.
/// </summary>
public interface IShippingCalculator
{
    /// <summary>
    /// Asynchronously calculates the shipping options and costs for the specified cart and delivery location.
    /// </summary>
    /// <param name="cart">The shopping cart containing the items for which to calculate shipping. Cannot be null.</param>
    /// <param name="deliveryLocation">The destination location where the order will be delivered. Cannot be null.</param>
    /// <param name="ctk">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a ShippingCalculatorResult with the
    /// available shipping options and their associated costs.</returns>
    Task<ShippingCalculatorResult> CalculateAsync(
        Cart cart,
        Point deliveryLocation,
        CancellationToken ctk = default);
}
