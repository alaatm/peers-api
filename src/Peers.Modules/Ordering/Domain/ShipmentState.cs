namespace Peers.Modules.Ordering.Domain;

/// <summary>
/// Shipment lifecycle states.
/// </summary>
public enum ShipmentState
{
    /// <summary>
    /// The shipment is being created and has not yet been finalized.
    /// </summary>
    Draft,
    /// <summary>
    /// The shipment has been prepared, and the shipping label has been generated.
    /// </summary>
    /// <remarks>
    /// Applicable only for platform-managed shipments.
    /// </remarks>
    LabelReady,
    /// <summary>
    /// The shipment has been dispatched and is currently in transit to the recipient.
    /// </summary>
    InTransit,
    /// <summary>
    /// The shipment has been successfully delivered to the recipient.
    /// </summary>
    Delivered,
    /// <summary>
    /// The delivery attempt was unsuccessful, and the shipment could not be delivered.
    /// </summary>
    DeliveryFailed,
    /// <summary>
    /// The shipment process is complete and no further actions are expected.
    /// </summary>
    Closed,
}
