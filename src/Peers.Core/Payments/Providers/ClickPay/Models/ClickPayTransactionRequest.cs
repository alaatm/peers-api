using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Peers.Core.Common;

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
    /// <param name="amount">The amount to be charged.</param>
    /// <param name="token">The token of the card to be charged.</param>
    /// <param name="description">The description of the transaction.</param>
    /// <param name="metadata">The metadata associated with the transaction.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateSale(
        string profileId,
        decimal amount,
        string token,
        string description,
        [NotNull] Dictionary<string, string> metadata) => Create
    (
        profileId,
        tranType: "sale",
        tranClass: "recurring",
        cartId: metadata["booking"],
        description: description,
        metadata: metadata,
        amount: amount,
        token: token
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for an authorization transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="amount">The amount to be authorized.</param>
    /// <param name="token">The token of the card to be authorized.</param>
    /// <param name="description">The description of the transaction.</param>
    /// <param name="metadata">The metadata associated with the transaction.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateAuthorization(
        string profileId,
        decimal amount,
        string token,
        string description,
        [NotNull] Dictionary<string, string> metadata) => Create
    (
        profileId,
        tranType: "auth",
        tranClass: "recurring",
        cartId: metadata["booking"],
        description: description,
        metadata: metadata,
        amount: amount,
        token: token
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a capture transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="amount">The amount to be captured.</param>
    /// <param name="description">The description of the transaction.</param>
    /// <param name="metadata">The metadata associated with the transaction.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateCapture(
        string profileId,
        string paymentId,
        decimal amount,
        string description,
        Dictionary<string, string>? metadata) => Create
    (
        profileId,
        tranType: "capture",
        tranClass: "ecom",
        cartId: metadata?.TryGetValue("booking", out var value) == true ? value : Guid.NewGuid().ToString(),
        description: description,
        metadata: metadata,
        amount: amount,
        tranRef: paymentId
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a void transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="amount">The amount to be voided. Must be exactly equal to the full authorized amount.</param>
    /// <param name="description">The description of the transaction.</param>
    /// <param name="metadata">The metadata associated with the transaction.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateVoid(
        string profileId,
        string paymentId,
        decimal amount,
        string description,
        Dictionary<string, string>? metadata) => Create
    (
        profileId,
        tranType: "void",
        tranClass: "ecom",
        cartId: metadata?.TryGetValue("booking", out var value) == true ? value : Guid.NewGuid().ToString(),
        description: description,
        metadata: metadata,
        amount: amount,
        tranRef: paymentId
    );

    /// <summary>
    /// Creates a new instance of the <see cref="ClickPayTransactionRequest"/> class for a refund transaction.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="amount">The amount to be refunded.</param>
    /// <param name="description">The description of the transaction.</param>
    /// <param name="metadata">The metadata associated with the transaction.</param>
    /// <returns></returns>
    public static ClickPayTransactionRequest CreateRefund(
        string profileId,
        string paymentId,
        decimal amount,
        string description,
        Dictionary<string, string>? metadata) => Create
    (
        profileId,
        tranType: "refund",
        tranClass: "ecom",
        cartId: metadata?.TryGetValue("booking", out var value) == true ? value : Guid.NewGuid().ToString(),
        description: description,
        metadata: metadata,
        amount: amount,
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
        Dictionary<string, string>? metadata = null)
    {
        if (amount?.GetDecimalPlaces() > 2)
        {
            throw new ArgumentException("Amount must be in SAR and have a maximum of 2 decimal places.", nameof(amount));
        }

        return new ClickPayTransactionRequest
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
}
