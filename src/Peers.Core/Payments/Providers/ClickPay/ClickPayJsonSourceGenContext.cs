using System.Text.Json.Serialization;
using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Payments.Providers.ClickPay;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = false,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(ClickPayHostedPagePaymentRequest))]
[JsonSerializable(typeof(ClickPayTransactionRequest))]
[JsonSerializable(typeof(ClickPayHostedPagePaymentResponse))]
[JsonSerializable(typeof(ClickPayHostedPageCallbackResponse))]
[JsonSerializable(typeof(ClickPayPaymentResponse))]
[JsonSerializable(typeof(ClickPayTokenResponse))]
[JsonSerializable(typeof(ClickPayErrorResponse))]
public partial class ClickPayJsonSourceGenContext : JsonSerializerContext
{
}
