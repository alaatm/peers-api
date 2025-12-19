using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Peers.Core.Payments.Providers.ClickPay.Models;

public sealed class ClickPayHostedPagePaymentRequest
{
    [JsonPropertyName("profile_id")]
    public string ProfileId { get; set; } = default!;

    [JsonPropertyName("tran_type")]
    public string TranType { get; set; } = default!;

    [JsonPropertyName("tran_class")]
    public string TranClass { get; } = "ecom";

    [JsonPropertyName("cart_id")]
    public string CartId { get; set; } = default!;

    [JsonPropertyName("cart_description")]
    public string CartDescription { get; set; } = default!;

    [JsonPropertyName("cart_amount")]
    public decimal CartAmount { get; set; }

    [JsonPropertyName("cart_currency")]
    public string CartCurrency { get; } = "SAR";

    [JsonPropertyName("paypage_lang")]
    public string Lang { get; set; } = default!;

    [JsonPropertyName("return")]
    public Uri ReturnUrl { get; set; } = default!;

    [JsonPropertyName("callback")]
    public Uri CallbackUrl { get; set; } = default!;

    [JsonPropertyName("customer_details")]
    public ClickPayCustomerDetails Customer { get; set; } = default!;

    [JsonPropertyName("user_defined")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("tokenize")]
    public int? Tokenize { get; set; }

    [JsonPropertyName("hide_shipping")]
    public bool HideShipping { get; } = true;

    [JsonPropertyName("show_save_card")]
    public bool ShowSaveCard { get; } = true;

    [JsonPropertyName("framed")]
    public bool Framed { get; } = false;

    public static ClickPayHostedPagePaymentRequest Create(
        string profileId,
        string lang,
        bool authOnly,
        bool tokenize,
        Uri returnUrl,
        Uri callbackUrl,
        [NotNull] PaymentInfo paymentInfo) => new()
        {
            ProfileId = profileId,
            TranType = authOnly ? "auth" : "sale",
            CartId = paymentInfo.OrderId,
            CartDescription = paymentInfo.Description,
            CartAmount = paymentInfo.Amount,
            Lang = lang,
            ReturnUrl = returnUrl,
            CallbackUrl = callbackUrl,
            Customer = new ClickPayCustomerDetails()
            {
                Phone = paymentInfo.CustomerPhone!,
                Email = paymentInfo.CustomerEmail!,
                Street1 = "Riyadh",
                City = "Riyadh",
                Country = "SA",
                Zip = "12345",
            },
            Metadata = paymentInfo.Metadata,
            Tokenize = tokenize ? 2 : null,
        };
}

