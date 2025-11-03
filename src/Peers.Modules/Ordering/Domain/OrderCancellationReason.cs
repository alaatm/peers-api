namespace Peers.Modules.Ordering.Domain;

/// <summary>
/// Specifies the reason for the cancellation of an order.
/// </summary>
public enum OrderCancellationReason
{
    /// <summary>
    /// Cancellation due to cart modification by the buyer.
    /// </summary>
    Amended,
}
