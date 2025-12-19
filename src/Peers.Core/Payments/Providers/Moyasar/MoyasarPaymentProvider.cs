using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Net.Http.Headers;
using Peers.Core.Common;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar.Configuration;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Core.Payments.Providers.Moyasar.Models.Payouts;

namespace Peers.Core.Payments.Providers.Moyasar;

/// <summary>
/// Represents a payment provider using Moyasar service.
/// </summary>
public sealed class MoyasarPaymentProvider : IPaymentProvider
{
    public const string Name = "moyasar";

    private const string ApiVersion = "v1";
    private const string ApiBaseUrl = "https://api.moyasar.com";
    private const string PaymentsEndpoint = "payments";
    private const string CapturePaymentEndpoint = "payments/{0}/capture";
    private const string VoidPaymentEndpoint = "payments/{0}/void";
    private const string RefundPaymentEndpoint = "payments/{0}/refund";
    private const string FetchPaymentEndpoint = "payments/{0}";
    private const string FetchTokenEndpoint = "tokens/{0}";
    private const string DeleteTokenEndpoint = "tokens/{0}";
    private const string PayoutBulkEndpoint = "payouts/bulk";
    private const string FetchPayoutEndpoint = "payouts/{0}";

    private readonly HttpClient _httpClient;
    private readonly MoyasarConfig _config;

    public string ProviderName => Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoyasarPaymentProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="config">The configuration.</param>
    public MoyasarPaymentProvider(
        [NotNull] HttpClient httpClient,
        [NotNull] MoyasarConfig config)
    {
        var keyBytes = Encoding.UTF8.GetBytes(config.Key);
        var key = Convert.ToBase64String(keyBytes);

        _config = config;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri($"{ApiBaseUrl}/{ApiVersion}/");
        _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", key);
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
    public Task<HostedPagePaymentInitResponse> InitiateHostedPagePaymentAsync(
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

        var metadata = paymentInfo.Metadata ?? [];
        metadata[PaymentInfo.OrderIdKey] = paymentInfo.OrderId;

        var script = $$"""
            Moyasar.init({
                element: '.payment-form',
                language: '{{language}}',
                publishable_api_key: '{{_config.PublishableKey}}',
                amount: {{(int)(paymentInfo.Amount * 100)}},
                currency: 'SAR',
                methods: ['creditcard'],
                credit_card: { save_card: {{JsonSerializer.Serialize(tokenize)}}, manual: {{JsonSerializer.Serialize(authOnly)}} },
                description: {{JsonSerializer.Serialize(paymentInfo.Description)}},
                metadata: {{JsonSerializer.Serialize(metadata)}},
                callback_url: '{{returnUrl}}',
                on_completed: async function (payment) {
                    await fetch("{{callbackUrl}}", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(payment),
                    });
                },
            });
            """;

        return Task.FromResult(new HostedPagePaymentInitResponse
        {
            Script = script,
        });
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

        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            PaymentsEndpoint,
            MoyasarPaymentRequest.Create(paymentType, true, token, paymentInfo));

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

        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            PaymentsEndpoint,
            MoyasarPaymentRequest.Create(paymentType, false, token, paymentInfo));

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

        await UpdatePaymentMetadataAsync(paymentId, paymentInfo.Description, paymentInfo.Metadata);

        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            Format(CapturePaymentEndpoint, paymentId),
            MoyasarCaptureRequest.Create(paymentInfo.Amount));

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

        await UpdatePaymentMetadataAsync(paymentId, paymentInfo.Description, paymentInfo.Metadata);

        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            Format(VoidPaymentEndpoint, paymentId),
            null);

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

        await UpdatePaymentMetadataAsync(paymentId, paymentInfo.Description, paymentInfo.Metadata);

        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            Format(RefundPaymentEndpoint, paymentId),
            MoyasarRefundRequest.Create(paymentInfo.Amount));

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
        catch (MoyasarException)
        {
            return await RefundPaymentAsync(paymentId, paymentInfo);
        }
    }

    private async Task<MoyasarPaymentResponse?> UpdatePaymentMetadataAsync(string paymentId, string description, Dictionary<string, string>? metadata)
    {
        if (metadata?.Count > 0)
        {
            return await SendRequestAsync<MoyasarPaymentResponse>(
                HttpMethod.Put,
                Format(FetchPaymentEndpoint, paymentId),
                MoyasarUpdatePaymentRequest.Create(description, metadata));
        }

        return null;
    }

    /// <summary>
    /// Fetches a payment details by id.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <returns></returns>
    public async Task<PaymentResponse?> FetchPaymentAsync(string paymentId)
    {
        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Get,
            Format(FetchPaymentEndpoint, paymentId),
            null);

        return response?.ToGeneric();
    }

    /// <summary>
    /// Fetches payment card details by the token id.
    /// </summary>
    /// <param name="tokenId">The token id.</param>
    /// <returns></returns>
    public async Task<TokenResponse?> FetchTokenAsync(string tokenId)
    {
        var response = await SendRequestAsync<MoyasarTokenResponse>(
            HttpMethod.Get,
            Format(FetchTokenEndpoint, tokenId),
            null);

        return response?.ToGeneric();
    }

    /// <summary>
    /// Deletes a tokenized payment card
    /// </summary>
    /// <param name="tokenId">The token id.</param>
    /// <returns></returns>
    public Task DeleteTokenAsync(string tokenId)
        => SendRequestAsync(
            HttpMethod.Delete,
            Format(DeleteTokenEndpoint, tokenId),
            null);

    /// <summary>
    /// Initiates a bulk payout request with the given details.
    /// </summary>
    /// <param name="request">The payout request.</param>
    /// <returns></returns>
    public async Task<PayoutResponse> SendPayoutsAsync(PayoutRequest request)
    {
        var response = await SendRequestAsync<MoyasarPayoutResponse>(
            HttpMethod.Post,
            PayoutBulkEndpoint,
            MoyasarPayoutRequest.Create(_config.PayoutAccountId, request));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Retrieves the status of a payout request.
    /// </summary>
    /// <param name="payoutId">The payout id.</param>
    /// <returns></returns>
    public async Task<PayoutResponseEntry?> FetchPayoutAsync(string payoutId)
    {
        var response = await SendRequestAsync<MoyasarPayoutResponseEntry>(
            HttpMethod.Get,
            Format(FetchPayoutEndpoint, payoutId),
            null);

        return response?.ToGeneric();
    }

    private async Task<TResponse?> SendRequestAsync<TResponse>(HttpMethod method, string url, object? body)
        where TResponse : class
    {
        var response = await SendRequestCoreAsync(method, url, body);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return (await response.Content.ReadFromJsonAsync<TResponse>(MoyasarJsonSourceGenContext.Default.Options))!;
    }

    private Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, object? body)
        => SendRequestCoreAsync(method, url, body);

    private async Task<HttpResponseMessage> SendRequestCoreAsync(HttpMethod method, string url, object? body)
    {
        using var request = new HttpRequestMessage(method, url)
        {
            Content = body is not null
                ? JsonContent.Create(body, body.GetType(), options: MoyasarJsonSourceGenContext.Default.Options)
                : null,
        };

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode is System.Net.HttpStatusCode.BadRequest)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync(MoyasarJsonSourceGenContext.Default.MoyasarErrorResponse);
            throw new MoyasarException("Moyasar API call failed.", errorResponse!);
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

    private static string Format(string format, params object[] args)
        => string.Format(CultureInfo.InvariantCulture, format, args);
}
