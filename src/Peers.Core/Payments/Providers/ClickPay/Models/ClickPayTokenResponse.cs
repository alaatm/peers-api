using System.Text.Json.Serialization;
using Peers.Core.Payments.Models;

namespace Peers.Core.Payments.Providers.ClickPay.Models;

public sealed class ClickPayTokenResponse
{
    [JsonPropertyName("tran_ref")]
    public string TranRef { get; set; } = default!;

    [JsonPropertyName("payment_info")]
    public ClickPayPaymentInfo PaymentInfo { get; set; } = default!;

    [JsonPropertyName("customer_details")]
    public ClickPayCustomerDetails CustomerDetails { get; set; } = default!;

    public TokenResponse ToGeneric() => new()
    {
        CardBrand = PaymentCardUtils.ResolveCardBrand(PaymentInfo.CardScheme),
        CardType = PaymentCardUtils.ResolveCardFunding(PaymentInfo.CardType),
        MaskedCardNumber = PaymentInfo.PaymentDescription,
        ExpiryMonth = PaymentInfo.ExpiryMonth,
        ExpiryYear = PaymentInfo.ExpiryYear
    };
}
