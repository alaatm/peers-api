using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Peers.Core.Payments.Models;

namespace Peers.Core.Payments.Providers.Moyasar.Models;

/// <summary>
/// Represents a payment request.
/// </summary>
public sealed class MoyasarPaymentRequest
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "SAR";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("callback_url")]
    public Uri? CallbackUrl { get; set; }

    [JsonPropertyName("source")]
    public MoyasarPaymentSource Source { get; set; } = default!;

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="MoyasarPaymentRequest"/>.
    /// </summary>
    /// <param name="type">The payment source type.</param>
    /// <param name="immediateCapture">Whether the amount should be captured immediately, or not (i.e authorize only).</param>
    /// <param name="token">The card or apple pay token.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static MoyasarPaymentRequest Create(PaymentSourceType type, bool immediateCapture, string token, [NotNull] PaymentInfo paymentInfo)
    {
        var manual = immediateCapture ? "false" : "true";
        var metadata = paymentInfo.Metadata ?? [];
        metadata[PaymentInfo.OrderIdKey] = paymentInfo.OrderId;

        var paymentRequest = new MoyasarPaymentRequest
        {
            Amount = (int)(paymentInfo.Amount * 100),
            Description = paymentInfo.Description,
            Metadata = metadata,
        };

        if (type is PaymentSourceType.ApplePay)
        {
            paymentRequest.Source = new MoyasarApplePayPaymentSource
            {
                Manual = manual,
                Token = token,
            };
        }
        else if (type is PaymentSourceType.TokenizedCard)
        {
            paymentRequest.Source = new MoyasarTokenPaymentSource
            {
                Manual = manual,
                Token = token,
                ThreeDSecure = false,
            };
        }
        else
        {
            throw new NotSupportedException($"Payment type {type} is not supported.");
        }

        return paymentRequest;
    }
}

/// <summary>
/// Represents a payment request source.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(MoyasarTokenPaymentSource), "token")]
[JsonDerivedType(typeof(MoyasarApplePayPaymentSource), "applepay")]
public abstract class MoyasarPaymentSource
{
    [JsonPropertyName("manual")]
    public string Manual { get; set; } = default!;
}

/// <summary>
/// Represents a tokenized payment request source.
/// </summary>
public sealed class MoyasarTokenPaymentSource : MoyasarPaymentSource
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = default!;

    [JsonPropertyName("3ds")]
    public bool ThreeDSecure { get; set; }
}

/// <summary>
/// Represents an apple pay payment request source.
/// </summary>
public sealed class MoyasarApplePayPaymentSource : MoyasarPaymentSource
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = default!;
}
