using System.Globalization;
using System.Text.Json.Serialization;
using Peers.Core.Payments.Models;

namespace Peers.Core.Payments.Providers.ClickPay.Models;

public sealed class ClickPayPaymentResponse
{
    public const string StatusPaid = "Sale";
    public const string StatusAuth = "Authorize";
    public const string StatusCapture = "Capture";
    public const string StatusRefund = "Refund";
    public const string StatusVoid = "Void";

    [JsonPropertyName("tran_ref")]
    public string TranRef { get; set; } = default!;

    [JsonPropertyName("previous_tran_ref")]
    public string? PreviousTranRef { get; set; }

    [JsonPropertyName("tran_type")]
    public string TranType { get; set; } = default!;

    [JsonPropertyName("original_tran_type")]
    public string? OriginalTranType { get; set; }

    [JsonPropertyName("cart_id")]
    public string CartId { get; set; } = default!;

    [JsonPropertyName("cart_description")]
    public string CartDescription { get; set; } = default!;

    [JsonPropertyName("cart_currency")]
    public string CartCurrency { get; set; } = default!;

    [JsonPropertyName("cart_amount")]
    public string CartAmount { get; set; } = default!;

    [JsonPropertyName("tran_currency")]
    public string TranCurrency { get; set; } = default!;

    [JsonPropertyName("tran_total")]
    public string TranTotal { get; set; } = default!;

    [JsonPropertyName("customer_details")]
    public ClickPayCustomerDetails CustomerDetails { get; set; } = default!;

    [JsonPropertyName("payment_result")]
    public ClickPayPaymentResult PaymentResult { get; set; } = default!;

    [JsonPropertyName("payment_info")]
    public ClickPayPaymentInfo PaymentInfo { get; set; } = default!;

    [JsonPropertyName("serviceId")]
    public int ServiceId { get; set; }

    [JsonPropertyName("paymentChannel")]
    public string PaymentChannel { get; set; } = default!;

    [JsonPropertyName("profileId")]
    public int ProfileId { get; set; }

    [JsonPropertyName("merchantId")]
    public int MerchantId { get; set; }

    [JsonPropertyName("trace")]
    public string Trace { get; set; } = default!;

    [JsonPropertyName("parentRequest")]
    public ClickPayParentRequest? ParentRequest { get; set; }

    public PaymentResponse ToGeneric()
    {
        var operation = TranType switch
        {
            StatusPaid => PaymentOperationType.Payment,
            StatusAuth => PaymentOperationType.Authorization,
            StatusCapture => PaymentOperationType.Capture,
            StatusRefund => PaymentOperationType.Refund,
            StatusVoid => PaymentOperationType.Void,
            _ => PaymentOperationType.Unknown,
        };

        var isSuccessful = PaymentResult.ResponseStatus is "A";

        return new PaymentResponse
        {
            PaymentId = TranRef,
            ParentPaymentId = PreviousTranRef,
            Operation = operation,
            Amount = decimal.Parse(CartAmount, CultureInfo.InvariantCulture),
            Currency = CartCurrency,
            Timestamp = PaymentResult.TransactionTime,
            IsSuccessful = isSuccessful,
            ProviderSpecificResponse = this,
        };
    }
}

public sealed class ClickPayCustomerDetails
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = default!;

    [JsonPropertyName("street1")]
    public string Street1 { get; set; } = default!;

    [JsonPropertyName("city")]
    public string City { get; set; } = default!;

    [JsonPropertyName("state")]
    public string State { get; set; } = default!;

    [JsonPropertyName("country")]
    public string Country { get; set; } = default!;

    [JsonPropertyName("zip")]
    public string Zip { get; set; } = default!;

    [JsonPropertyName("ip")]
    public string? IP { get; set; } = default!;
}

public sealed class ClickPayPaymentInfo
{
    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = default!;

    [JsonPropertyName("card_type")]
    public string CardType { get; set; } = default!;

    [JsonPropertyName("card_scheme")]
    public string CardScheme { get; set; } = default!;

    [JsonPropertyName("payment_description")]
    public string PaymentDescription { get; set; } = default!;

    [JsonPropertyName("expiryMonth")]
    public int ExpiryMonth { get; set; }

    [JsonPropertyName("expiryYear")]
    public int ExpiryYear { get; set; }
}

public sealed class ClickPayPaymentResult
{
    [JsonPropertyName("response_status")]
    public string ResponseStatus { get; set; } = default!;

    [JsonPropertyName("response_code")]
    public string ResponseCode { get; set; } = default!;

    [JsonPropertyName("response_message")]
    public string ResponseMessage { get; set; } = default!;

    [JsonPropertyName("transaction_time")]
    public DateTime TransactionTime { get; set; }
}

public sealed class ClickPayParentRequest
{
    [JsonPropertyName("tran_ref")]
    public string TranRef { get; set; } = default!;

    [JsonPropertyName("cart_currency")]
    public string CartCurrency { get; set; } = default!;

    [JsonPropertyName("cart_amount")]
    public string CartAmount { get; set; } = default!;
}
