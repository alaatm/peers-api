using System.Text.Json.Serialization;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Core.Payments.Providers.Moyasar.Models.Payouts;

namespace Peers.Core.Payments.Providers.Moyasar;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = false,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(MoyasarPaymentRequest))]
[JsonSerializable(typeof(MoyasarCaptureRequest))]
[JsonSerializable(typeof(MoyasarRefundRequest))]
[JsonSerializable(typeof(MoyasarUpdatePaymentRequest))]
[JsonSerializable(typeof(MoyasarPayoutRequest))]
[JsonSerializable(typeof(MoyasarPaymentResponse))]
[JsonSerializable(typeof(MoyasarTokenResponse))]
[JsonSerializable(typeof(MoyasarPayoutResponse))]
[JsonSerializable(typeof(MoyasarPayoutResponseEntry))]
[JsonSerializable(typeof(MoyasarErrorResponse))]
public partial class MoyasarJsonSourceGenContext : JsonSerializerContext
{
}
