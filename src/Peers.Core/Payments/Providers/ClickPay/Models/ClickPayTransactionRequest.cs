using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Peers.Core.Payments.Providers.ClickPay.Models;

/// <summary>
/// Represents a ClickPay transaction request.
/// </summary>
public sealed class ClickPayTransactionRequest
{
    [JsonPropertyName("profile_id")]
    public string ProfileId { get; set; } = default!;

    [JsonPropertyName("tran_type")]
    public string? TransactionType { get; set; }

    [JsonPropertyName("tran_class")]
    public string? TransactionClass { get; set; }

    [JsonPropertyName("cart_id")]
    public string? CartId { get; set; }

    [JsonPropertyName("cart_description")]
    public string? Description { get; set; }

    [JsonPropertyName("cart_amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("cart_currency")]
    public string? Currency { get; set; } = "SAR";

    [JsonPropertyName("tran_ref")]
    public string? TransactionRef { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("user_defined")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a sale transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="token">The token of the card to be charged.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateSale(
        string profileId,
        string token,
        [NotNull] PaymentInfo paymentInfo) => Create
    (
        profileId,
        tranType: "sale",
        tranClass: "recurring",
        cartId: paymentInfo.OrderId,
        description: paymentInfo.Description,
        metadata: paymentInfo.Metadata,
        amount: paymentInfo.Amount,
        token: token
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for an authorization transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="token">The token of the card to be authorized.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateAuthorization(
        string profileId,
        string token,
        [NotNull] PaymentInfo paymentInfo) => Create
    (
        profileId,
        tranType: "auth",
        tranClass: "recurring",
        cartId: paymentInfo.OrderId,
        description: paymentInfo.Description,
        metadata: paymentInfo.Metadata,
        amount: paymentInfo.Amount,
        token: token
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a capture transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateCapture(
        string profileId,
        string paymentId,
        [NotNull] PaymentInfo paymentInfo) => Create
    (
        profileId,
        tranType: "capture",
        tranClass: "ecom",
        cartId: paymentInfo.OrderId,
        description: paymentInfo.Description,
        metadata: paymentInfo.Metadata,
        amount: paymentInfo.Amount,
        tranRef: paymentId
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a void transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateVoid(
        string profileId,
        string paymentId,
        [NotNull] PaymentInfo paymentInfo) => Create
    (
        profileId,
        tranType: "void",
        tranClass: "ecom",
        cartId: paymentInfo.OrderId,
        description: paymentInfo.Description,
        metadata: paymentInfo.Metadata,
        amount: paymentInfo.Amount,
        tranRef: paymentId
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a refund transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateRefund(
        string profileId,
        string paymentId,
        [NotNull] PaymentInfo paymentInfo) => Create
    (
        profileId,
        tranType: "refund",
        tranClass: "ecom",
        cartId: paymentInfo.OrderId,
        description: paymentInfo.Description,
        metadata: paymentInfo.Metadata,
        amount: paymentInfo.Amount,
        tranRef: paymentId
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a payment query.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="paymentId">The payment ID.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateQuery(
        string profileId,
        string paymentId) => Create
    (
        profileId,
        tranRef: paymentId
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a token query/delete.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="token">The token of the card to be queried or deleted.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateTokenQueryOrDelete(
        string profileId,
        string token) => Create
    (
        profileId,
        token: token
    );

    private static ClickPayTransactionRequest Create(
        string profileId,
        string? tranType = null,
        string? tranClass = null,
        string? cartId = null,
        string? description = null,
        decimal? amount = null,
        string? tranRef = null,
        string? token = null,
        Dictionary<string, string>? metadata = null) => new()
        {
            ProfileId = profileId,
            TransactionType = tranType,
            TransactionClass = tranClass,
            CartId = cartId,
            Description = description,
            Amount = amount,
            Token = token,
            TransactionRef = tranRef,
            Metadata = metadata,
        };
}
