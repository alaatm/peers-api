using System.ComponentModel.DataAnnotations.Schema;

namespace Peers.Core.Payments.Models;

/// <summary>
/// Represents a generic payment response.
/// </summary>
public sealed class PaymentResponse : IEquatable<PaymentResponse>
{
    /// <summary>
    /// The unique identifier for the payment operation.
    /// </summary>
    public required string PaymentId { get; init; } = default!;
    /// <summary>
    /// The unique identifier for the payment operation that is the parent of this operation.
    /// </summary>
    /// <remarks>
    /// For Moyasar, all operations on the same payment share the same parent ID.
    /// For ClickPay, the parent ID is the ID of the parent operation.
    /// e.g, Auth followed by Capture followed by Refund. Auth is the parent, Capture and Refund will have their Parent Payment ID set to Auth Payment ID.
    /// </remarks>
    public string? ParentPaymentId { get; init; }
    /// <summary>
    /// The type of payment operation (e.g., authorization, capture, refund).
    /// </summary>
    public required PaymentOperationType Operation { get; init; }
    /// <summary>
    /// The amount of money involved in the payment operation.
    /// </summary>
    public required decimal Amount { get; init; }
    /// <summary>
    /// The currency in which the payment is made (e.g., USD, EUR).
    /// </summary>
    public required string Currency { get; init; } = default!;
    /// <summary>
    /// The timestamp of when the payment operation was performed.
    /// </summary>
    public required DateTime Timestamp { get; init; }
    /// <summary>
    /// Indicates whether the payment operation was successful.
    /// </summary>
    public required bool IsSuccessful { get; init; }
    /// <summary>
    /// The raw response object from the payment provider.
    /// </summary>
    [NotMapped]
    public object? ProviderSpecificResponse { get; set; }
    /// <summary>
    /// User-defined errors that occurred after attempting to post-process the payment.
    /// </summary>
    public List<string>? Errors { get; set; }

    public bool Equals(PaymentResponse? other) =>
        other is not null &&
        PaymentId == other.PaymentId &&
        ParentPaymentId == other.ParentPaymentId &&
        Operation == other.Operation &&
        Amount == other.Amount &&
        Currency == other.Currency &&
        Timestamp == other.Timestamp &&
        IsSuccessful == other.IsSuccessful;

    public override bool Equals(object? obj)
        => obj is PaymentResponse other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(PaymentId, ParentPaymentId, Operation, Amount, Currency, Timestamp, IsSuccessful);
}
