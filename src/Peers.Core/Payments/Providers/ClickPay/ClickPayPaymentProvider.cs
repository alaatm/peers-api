using System.Diagnostics.CodeAnalysis;
using Microsoft.Net.Http.Headers;
using Peers.Core.Common;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Payments.Providers.ClickPay;

/// <summary>
/// Represents a payment provider using ClickPay service.
/// </summary>
public sealed class ClickPayPaymentProvider : IPaymentProvider
{
    public const string Name = "clickpay";

    private const string ApiBaseUrl = "https://secure.clickpay.com.sa";
    private const string PaymentRequestEndpoint = "payment/request";
    private const string PaymentQueryEndpoint = "payment/query";
    private const string TokenQueryEndpoint = "payment/token";
    private const string TokenDeleteEndpoint = "payment/token/delete";

    private readonly HttpClient _httpClient;
    private readonly ClickPayConfig _config;

    public string ProviderName => Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickPayPaymentProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="config">The configuration.</param>
    public ClickPayPaymentProvider(
        [NotNull] HttpClient httpClient,
        [NotNull] ClickPayConfig config)
    {
        _config = config;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
        _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        _httpClient.DefaultRequestHeaders.Add("Authorization", _config.Key);
    }

    /// <summary>
    /// Initiates a hosted page card tokenization request with the given details.
    /// </summary>
    /// <param name="returnUrl">The return URL.</param>
    /// <param name="callbackUrl">The callback URL.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <param name="language">The desired language of the hosted page.</param>
    /// <returns></returns>
    public Task<HostedPagePaymentInitResponse> InitiateHostedPageTokenizationAsync(
        [NotNull] Uri returnUrl,
        [NotNull] Uri callbackUrl,
        [NotNull] PaymentInfo paymentInfo,
        string language)
    {
        ArgumentNullException.ThrowIfNull(paymentInfo, nameof(paymentInfo));

        if (paymentInfo.Intent is not PaymentInfoIntent.Tokenization)
        {
            throw new InvalidOperationException("PaymentInfo intent must be Tokenization for hosted page tokenization requests.");
        }

        paymentInfo.PromoteToHppIntent();

        return InitiateHostedPagePaymentAsync(
            returnUrl,
            callbackUrl,
            paymentInfo,
            true,
            true,
            language);
    }

    /// <summary>
    /// Initiates a hosted page payment request with the given details.
    /// </summary>
    /// <param name="returnUrl">The return URL.</param>
    /// <param name="callbackUrl">The callback URL.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <param name="authOnly">True for authorization only; otherwise for immediate capture.</param>
    /// <param name="tokenize">True to tokenize customer card; otheriwse false.</param>
    /// <param name="language">The desired language of the hosted page.</param>
    /// <returns></returns>
    public async Task<HostedPagePaymentInitResponse> InitiateHostedPagePaymentAsync(
        [NotNull] Uri returnUrl,
        [NotNull] Uri callbackUrl,
        [NotNull] PaymentInfo paymentInfo,
        bool authOnly,
        bool tokenize,
        string language)
    {
        ArgumentNullException.ThrowIfNull(paymentInfo, nameof(paymentInfo));

        if (paymentInfo.Intent is not PaymentInfoIntent.HostedPaymentPage)
        {
            throw new InvalidOperationException("PaymentInfo intent must be HostedPaymentPage for hosted page payment requests.");
        }

        ArgumentNullException.ThrowIfNull(returnUrl, nameof(returnUrl));
        ArgumentNullException.ThrowIfNull(callbackUrl, nameof(callbackUrl));
        ArgumentException.ThrowIfNullOrWhiteSpace(language, nameof(language));

        var response = await SendRequestAsync<ClickPayHostedPagePaymentResponse>(
            HttpMethod.Post,
            PaymentRequestEndpoint,
            ClickPayHostedPagePaymentRequest.Create(_config.ProfileId, language, authOnly, tokenize, returnUrl, callbackUrl, paymentInfo));

        return response is not null
            ? new HostedPagePaymentInitResponse { RedirectUrl = response.RedirectUrl }
            : new HostedPagePaymentInitResponse();
    }

    /// <summary>
    /// Creates a payment that will be captured immediately with the given details.
    /// </summary>
    /// <param name="paymentType">The payment source type.</param>
    /// <param name="token">The source token.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> CreatePaymentAsync(PaymentSourceType paymentType, string token, [NotNull] PaymentInfo paymentInfo)
    {
        ArgumentNullException.ThrowIfNull(paymentInfo, nameof(paymentInfo));

        if (paymentInfo.Intent is not PaymentInfoIntent.TransactionApi)
        {
            throw new InvalidOperationException("PaymentInfo intent must be TransactionApi for transaction API payment requests.");
        }

        var response = await SendRequestAsync<ClickPayPaymentResponse>(
            HttpMethod.Post,
            PaymentRequestEndpoint,
            ClickPayTransactionRequest.CreateSale(_config.ProfileId, token, paymentInfo));

        return response!.ToGeneric();
    }


    /// <summary>
    /// Authorizes a payment with the given details.
    /// </summary>
    /// <param name="paymentType">The payment source type.</param>
    /// <param name="token">The source token.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> AuthorizePaymentAsync(PaymentSourceType paymentType, string token, [NotNull] PaymentInfo paymentInfo)
    {
        ArgumentNullException.ThrowIfNull(paymentInfo, nameof(paymentInfo));

        if (paymentInfo.Intent is not PaymentInfoIntent.TransactionApi)
        {
            throw new InvalidOperationException("PaymentInfo intent must be TransactionApi for transaction API payment requests.");
        }

        var response = await SendRequestAsync<ClickPayPaymentResponse>(
            HttpMethod.Post,
            PaymentRequestEndpoint,
            ClickPayTransactionRequest.CreateAuthorization(_config.ProfileId, token, paymentInfo));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Captures the specified amount from the authorized amount of the specified payment.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> CapturePaymentAsync(string paymentId, [NotNull] PaymentInfo paymentInfo)
    {
        ArgumentNullException.ThrowIfNull(paymentInfo, nameof(paymentInfo));

        if (paymentInfo.Intent is not PaymentInfoIntent.TransactionApi)
        {
            throw new InvalidOperationException("PaymentInfo intent must be TransactionApi for transaction API payment requests.");
        }

        var response = await SendRequestAsync<ClickPayPaymentResponse>(
            HttpMethod.Post,
            PaymentRequestEndpoint,
            ClickPayTransactionRequest.CreateCapture(_config.ProfileId, paymentId, paymentInfo));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Voids an authorized payment.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> VoidPaymentAsync(string paymentId, [NotNull] PaymentInfo paymentInfo)
    {
        ArgumentNullException.ThrowIfNull(paymentInfo, nameof(paymentInfo));

        if (paymentInfo.Intent is not PaymentInfoIntent.TransactionApi)
        {
            throw new InvalidOperationException("PaymentInfo intent must be TransactionApi for transaction API payment requests.");
        }

        var response = await SendRequestAsync<ClickPayPaymentResponse>(
            HttpMethod.Post,
            PaymentRequestEndpoint,
            ClickPayTransactionRequest.CreateVoid(_config.ProfileId, paymentId, paymentInfo));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Refunds the specified amount from the captured amount of the specified payment.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> RefundPaymentAsync(string paymentId, [NotNull] PaymentInfo paymentInfo)
    {
        ArgumentNullException.ThrowIfNull(paymentInfo, nameof(paymentInfo));

        if (paymentInfo.Intent is not PaymentInfoIntent.TransactionApi)
        {
            throw new InvalidOperationException("PaymentInfo intent must be TransactionApi for transaction API payment requests.");
        }

        var response = await SendRequestAsync<ClickPayPaymentResponse>(
            HttpMethod.Post,
            PaymentRequestEndpoint,
            ClickPayTransactionRequest.CreateRefund(_config.ProfileId, paymentId, paymentInfo));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Attempts to void a payment, if it's not possible, it refunds it.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="paymentInfo">The payment information.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> VoidOrRefundPaymentAsync(string paymentId, [NotNull] PaymentInfo paymentInfo)
    {
        try
        {
            return await VoidPaymentAsync(paymentId, paymentInfo);
        }
        catch (ClickPayException)
        {
            return await RefundPaymentAsync(paymentId, paymentInfo);
        }
    }

    /// <summary>
    /// Fetches a payment details by id.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <returns></returns>
    public async Task<PaymentResponse?> FetchPaymentAsync(string paymentId)
    {
        var response = await SendRequestAsync<ClickPayPaymentResponse>(
            HttpMethod.Post,
            PaymentQueryEndpoint,
            ClickPayTransactionRequest.CreateQuery(_config.ProfileId, paymentId));

        return response?.ToGeneric();
    }

    /// <summary>
    /// Fetches payment card details by the token id.
    /// </summary>
    /// <param name="tokenId">The token id.</param>
    /// <returns></returns>
    public async Task<TokenResponse?> FetchTokenAsync(string tokenId)
    {
        var response = await SendRequestAsync<ClickPayTokenResponse>(
            HttpMethod.Post,
            TokenQueryEndpoint,
            ClickPayTransactionRequest.CreateTokenQueryOrDelete(_config.ProfileId, tokenId));

        return response?.ToGeneric();
    }

    /// <summary>
    /// Deletes a tokenized payment card
    /// </summary>
    /// <param name="tokenId">The token id.</param>
    /// <returns></returns>
    public Task DeleteTokenAsync(string tokenId)
        => SendRequestAsync(
            HttpMethod.Post,
            TokenDeleteEndpoint,
            ClickPayTransactionRequest.CreateTokenQueryOrDelete(_config.ProfileId, tokenId));

    /// <summary>
    /// Initiates a bulk payout request with the given details.
    /// </summary>
    /// <param name="request">The payout request.</param>
    /// <returns></returns>
    public Task<PayoutResponse> SendPayoutsAsync(PayoutRequest request) => throw new NotImplementedException();

    /// <summary>
    /// Retrieves the status of a payout request.
    /// </summary>
    /// <param name="payoutId">The payout id.</param>
    /// <returns></returns>
    public Task<PayoutResponseEntry?> FetchPayoutAsync(string payoutId) => throw new NotImplementedException();

    private async Task<TResponse?> SendRequestAsync<TResponse>(HttpMethod method, string url, object body)
        where TResponse : class
    {
        var response = await SendRequestCoreAsync(method, url, body);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(ClickPayJsonSourceGenContext.Default.Options);
    }

    private Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, object body)
        => SendRequestCoreAsync(method, url, body);

    private async Task<HttpResponseMessage> SendRequestCoreAsync(HttpMethod method, string url, object body)
    {
        using var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(body, body.GetType(), options: ClickPayJsonSourceGenContext.Default.Options),
        };

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode is System.Net.HttpStatusCode.BadRequest)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync(ClickPayJsonSourceGenContext.Default.ClickPayErrorResponse);
            throw new ClickPayException("ClickPay API call failed.", errorResponse!);
        }
        else if (response.StatusCode is not System.Net.HttpStatusCode.NotFound)
        {
            // Throw if not a success status code other than:
            // 400 (Bad Request) - handled above
            // 404 (Not Found) - handled above
            // 408 (Request Timeout) - handled by Polly
            // 429 (Too many requests) - handled by Polly
            // 5XX (Server Error) - handled by Polly
            response.EnsureSuccessStatusCode();
        }

        return response;
    }
}
