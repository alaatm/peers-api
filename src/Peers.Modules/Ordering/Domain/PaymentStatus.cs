namespace Peers.Modules.Ordering.Domain;

/// <summary>
/// Indicates the status of an order payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Indicates that no payment has been made yet.
    /// </summary>
    Unpaid,
    /// <summary>
    /// Indicates that the payment is authorized.
    /// </summary>
    Authorized,
    /// <summary>
    /// Indicates that the payment is partially captured.
    /// </summary>
    PartiallyCaptured,
    /// <summary>
    /// Indicates that the payment is fully captured.
    /// </summary>
    Captured,
    /// <summary>
    /// Indicates that the payment is partially refunded.
    /// </summary>
    PartiallyRefunded,
    /// <summary>
    /// Indicates that the payment is fully refunded.
    /// </summary>
    Refunded,
    /// <summary>
    /// Indicates that the payment is voided.
    /// </summary>
    Voided,
}
