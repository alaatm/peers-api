namespace Peers.Modules.Ordering.Domain;

/// <summary>
/// Specifies the possible states of an order as it progresses through its lifecycle.
/// </summary>
public enum OrderState
{
    /// <summary>
    /// Indicates that the order has been placed by the buyer but not yet processed by the seller.
    /// </summary>
    Placed,
    ReadyToShip,
    InTransit,
    Delivered,
    DispatchBreached,
    Closed,
    /// <summary>
    /// Indicates that the order has been cancelled.
    /// </summary>
    Cancelled,
}
