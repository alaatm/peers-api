using Peers.Core.Payments.Models;

namespace Peers.Core.Payments;

/// <summary>
/// Represents a payment provider.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Gets the name of the payment provider.
    /// </summary>
    string ProviderName { get; }
    /// <summary>
    /// Initiates a hosted page card tokenization request with the given details.
    /// </summary>
    /// <param name="returnUrl">The return URL.</param>
    /// <param name="callbackUrl">The callback URL.</param>
    /// <param name="language">The desired language of the hosted page.</param>
    /// <param name="customerPhone">The customer phone number.</param>
    /// <param name="customerEmail">The customer email address.</param>
    /// <returns></returns>
    Task<HostedPagePaymentInitResponse> InitiateHostedPageTokenizationAsync(
        Uri returnUrl,
        Uri callbackUrl,
        string language,
        string customerPhone,
        string? customerEmail);
    /// <summary>
    /// Initiates a hosted page payment request with the given details.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <param name="returnUrl">The return URL.</param>
    /// <param name="callbackUrl">The callback URL.</param>
    /// <param name="authOnly">True for authorization only; otherwise for immediate capture.</param>
    /// <param name="tokenize">True to tokenize customer card; otheriwse false.</param>
    /// <param name="language">The desired language of the hosted page.</param>
    /// <param name="customerPhone">The customer phone number.</param>
    /// <param name="customerEmail">The customer email address.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    Task<HostedPagePaymentInitResponse> InitiateHostedPagePaymentAsync(
        decimal amount,
        Uri returnUrl,
        Uri callbackUrl,
        bool authOnly,
        bool tokenize,
        string language,
        string customerPhone,
        string? customerEmail,
        string description,
        Dictionary<string, string> metadata);
    /// <summary>
    /// Creates a payment that will be captured immediately with the given details.
    /// </summary>
    /// <param name="paymentType">The payment source type.</param>
    /// <param name="amount">The payment amount.</param>
    /// <param name="token">The source token.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    Task<PaymentResponse> CreatePaymentAsync(PaymentSourceType paymentType, decimal amount, string token, string description, Dictionary<string, string>? metadata);
    /// <summary>
    /// Authorizes a payment with the given details.
    /// </summary>
    /// <param name="paymentType">The payment source type.</param>
    /// <param name="amount">The authorize amount.</param>
    /// <param name="token">The source token.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    Task<PaymentResponse> AuthorizePaymentAsync(PaymentSourceType paymentType, decimal amount, string token, string description, Dictionary<string, string>? metadata);
    /// <summary>
    /// Captures the specified amount from the authorized amount of the specified payment.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="amount">The amount to capture.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    Task<PaymentResponse> CapturePaymentAsync(string paymentId, decimal amount, string description, Dictionary<string, string>? metadata);
    /// <summary>
    /// Voids an authorized payment.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="amount">The amount to void.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    Task<PaymentResponse> VoidPaymentAsync(string paymentId, decimal amount, string description, Dictionary<string, string>? metadata);
    /// <summary>
    /// Refunds the specified amount from the captured amount of the specified payment.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="amount">The amount to refund.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    Task<PaymentResponse> RefundPaymentAsync(string paymentId, decimal amount, string description, Dictionary<string, string>? metadata);
    /// <summary>
    /// Attempts to void a payment, if it's not possible, it refunds it.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="amount">The amount to void or refund.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    Task<PaymentResponse> VoidOrRefundPaymentAsync(string paymentId, decimal amount, string description, Dictionary<string, string>? metadata);
    /// <summary>
    /// Fetches a payment details by id.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <returns></returns>
    Task<PaymentResponse?> FetchPaymentAsync(string paymentId);
    /// <summary>
    /// Fetches payment card details by the token id.
    /// </summary>
    /// <param name="tokenId">The token id.</param>
    /// <returns></returns>
    Task<TokenResponse?> FetchTokenAsync(string tokenId);
    /// <summary>
    /// Deletes a tokenized payment card
    /// </summary>
    /// <param name="tokenId">The token id.</param>
    /// <returns></returns>
    Task DeleteTokenAsync(string tokenId);
    /// <summary>
    /// Initiates a bulk payout request with the given details.
    /// </summary>
    /// <param name="request">The payout request.</param>
    /// <returns></returns>
    Task<PayoutResponse> SendPayoutsAsync(PayoutRequest request);
    /// <summary>
    /// Retrieves the status of a payout request.
    /// </summary>
    /// <param name="payoutId">The payout id.</param>
    /// <returns></returns>
    Task<PayoutResponseEntry?> FetchPayoutAsync(string payoutId);
}
