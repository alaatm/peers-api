using System.Text.Json.Serialization;

namespace Peers.Core.Payments.Providers.ClickPay.Models;

public sealed class ClickPayHostedPagePaymentResponse
{
    [JsonPropertyName("redirect_url")]
    public Uri RedirectUrl { get; set; } = default!;
}

