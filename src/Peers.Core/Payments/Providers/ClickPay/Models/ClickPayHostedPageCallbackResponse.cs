using System.Text.Json.Serialization;

namespace Peers.Core.Payments.Providers.ClickPay.Models;

public sealed class ClickPayHostedPageCallbackResponse
{
    [JsonPropertyName("tran_ref")]
    public string TranRef { get; set; } = default!;

    [JsonPropertyName("token")]
    public string Token { get; set; } = default!;

    [JsonPropertyName("cart_id")]
    public string CartId { get; set; } = default!;

    [JsonPropertyName("payment_result")]
    public ClickPayPaymentResult PaymentResult { get; set; } = default!;

    [JsonPropertyName("payment_info")]
    public ClickPayPaymentInfo PaymentInfo { get; set; } = default!;
}
