namespace Peers.Core.Shipping;

/// <summary>
/// Represents an item included in a platform shipment, including its physical dimensions, weight, and quantity.
/// </summary>
/// <param name="Weight">The weight of the item, in kilograms. Must be a non-negative value.</param>
/// <param name="Length">The length of the item, in centimeters. Must be a non-negative value.</param>
/// <param name="Width">The width of the item, in centimeters. Must be a non-negative value.</param>
/// <param name="Height">The height of the item, in centimeters. Must be a non-negative value.</param>
/// <param name="Quantity">The number of units of this item included in the shipment. Must be greater than or equal to 1.</param>
public sealed record PlatformShipmentItem(
    double Weight,
    double Length,
    double Width,
    double Height,
    int Quantity);
