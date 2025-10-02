using Peers.Core.Domain.Errors;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Shipping policy that grants free shipping when both a minimum order amount
/// and a maximum delivery distance are satisfied.
/// </summary>
/// <param name="MinOrder">
/// Minimum order amount (store currency) required to qualify. Must be non-negative.
/// </param>
/// <param name="MaxDistance">
/// Maximum delivery distance in meters within which free shipping applies. Must be a finite, non-negative value.
/// </param>
public sealed record FreeShippingPolicy(
    decimal MinOrder,
    double MaxDistance)
{
    /// <summary>
    /// Determines whether the specified order amount and delivery distance satisfy the minimum order and maximum
    /// delivery distance requirements.
    /// </summary>
    /// <param name="orderAmount">Total order amount in the store’s currency. Must be non-negative.</param>
    /// <param name="distanceMeters">Distance from the origin (ship-from) to the delivery location, in meters. Must be finite and non-negative.</param>
    /// <returns>true if the order amount meets the minimum requirement and the distance between the store and delivery location
    /// does not exceed the allowed maximum; otherwise, false.</returns>
    public bool IsSatisfiedBy(decimal orderAmount, double distanceMeters)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(orderAmount);
        ArgumentOutOfRangeException.ThrowIfNegative(distanceMeters);

        return
            orderAmount >= MinOrder &&
            distanceMeters <= MaxDistance;
    }

    /// <summary>
    /// Validates the policy’s structural invariants.
    /// </summary>
    internal void Validate()
    {
        if (MinOrder < 0)
        {
            throw new DomainException(E.Logistics.FreeShippingMinOrderMustBeNonNegative);
        }
        if (MaxDistance < 0)
        {
            throw new DomainException(E.Logistics.FreeShippingMaxDistanceMetersMustBeNonNegative);
        }
    }
}
