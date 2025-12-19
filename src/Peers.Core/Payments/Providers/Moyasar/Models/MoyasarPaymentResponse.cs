using System.Text.Json.Serialization;
using Peers.Core.Payments.Models;

namespace Peers.Core.Payments.Providers.Moyasar.Models;

/// <summary>
/// Represents a payment response.
/// </summary>
public class MoyasarPaymentResponse
{
    public const string StatusInitiated = "initiated";
    public const string StatusPaid = "paid";
    public const string StatusAuth = "authorized";
    public const string StatusCapture = "captured";
    public const string StatusRefund = "refunded";
    public const string StatusVoid = "voided";
    public const string StatusFailed = "failed";

    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("fee")]
    public int Fee { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("refunded")]
    public int Refunded { get; set; }

    [JsonPropertyName("refunded_at")]
    public DateTime? RefundedAt { get; set; }

    [JsonPropertyName("captured")]
    public int Captured { get; set; }

    [JsonPropertyName("captured_at")]
    public DateTime? CapturedAt { get; set; }

    [JsonPropertyName("voided_at")]
    public DateTime? VoidedAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount_format")]
    public string AmountFormat { get; set; } = default!;

    [JsonPropertyName("fee_format")]
    public string FeeFormat { get; set; } = default!;

    [JsonPropertyName("refunded_format")]
    public string RefundedFormat { get; set; } = default!;

    [JsonPropertyName("captured_format")]
    public string CapturedFormat { get; set; } = default!;

    [JsonPropertyName("invoice_id")]
    public string? InvoiceId { get; set; }

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = default!;

    [JsonPropertyName("callback_url")]
    public Uri? CallbackUrl { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("source")]
    public MoyasarPaymentResponseSource Source { get; set; } = default!;

    public PaymentResponse ToGeneric()
    {
        var (operation, amount, timestamp) = Status switch
        {
            StatusPaid => (PaymentOperationType.Payment, Amount, CreatedAt),
            StatusAuth => (PaymentOperationType.Authorization, Amount, CreatedAt),
            StatusCapture => (PaymentOperationType.Capture, Captured, CapturedAt!.Value),
            StatusRefund => (PaymentOperationType.Refund, Refunded, RefundedAt!.Value),
            StatusVoid => (PaymentOperationType.Void, 0, VoidedAt!.Value),
            _ => (PaymentOperationType.Unknown, 0, DateTime.MinValue),
        };

        var isSuccessful = Status is not StatusFailed;

        string? orderId = null;
        Metadata?.TryGetValue(PaymentInfo.OrderIdKey, out orderId);

        return new PaymentResponse
        {
            PaymentId = Id,
            OrderId = orderId,
            Operation = operation,
            Amount = amount / 100m,
            Currency = Currency,
            Timestamp = timestamp,
            IsSuccessful = isSuccessful,
            ProviderSpecificResponse = this,
        };
    }
}

/// <summary>
/// Represents a payment response source.
/// </summary>
public class MoyasarPaymentResponseSource
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("company")]
    public string Company { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("number")]
    public string Number { get; set; } = default!;

    [JsonPropertyName("gateway_id")]
    public string GatewayId { get; set; } = default!;

    [JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("transaction_url")]
    public Uri TransactionUrl { get; set; } = default!;
}
