namespace Peers.Core.Payments.Models;

/// <summary>
/// Represents the type of payment operation.
/// </summary>
public enum PaymentOperationType
{
    /// <summary>
    /// A payment operation (i.e. auth and capture) that is not explicitly defined.
    /// </summary>
    Payment,
    /// <summary>
    /// A payment operation that is an authorization.
    /// </summary>
    Authorization,
    /// <summary>
    /// A payment operation that is a capture.
    /// </summary>
    Capture,
    /// <summary>
    /// A payment operation that is a refund.
    /// </summary>
    Refund,
    /// <summary>
    /// A payment operation that is a void.
    /// </summary>
    Void,
    /// <summary>
    /// An unknown payment operation type.
    /// </summary>
    Unknown,
}
