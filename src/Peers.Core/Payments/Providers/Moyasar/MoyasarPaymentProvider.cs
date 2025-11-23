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
    /// <param name="language">The desired language of the hosted page.</param>
    /// <param name="customerPhone">The customer phone number.</param>
    /// <param name="customerEmail">The customer email address.</param>
    /// <returns></returns>
    public Task<HostedPagePaymentInitResponse> InitiateHostedPageTokenizationAsync(
        Uri returnUrl,
        Uri callbackUrl,
        string language,
        string customerPhone,
        string? customerEmail) => InitiateHostedPagePaymentAsync(
            1,
            returnUrl,
            callbackUrl,
            true,
            true,
            language,
            customerPhone,
            customerEmail,
            "Tokenize customer card",
            new Dictionary<string, string>
            {
                ["customer"] = customerPhone,
            });

    /// <summary>
    /// Initiates a hosted page payment request with the given details.
    /// </summary>
    /// <param name="amount">The payment amount.</param>
    /// <param name="returnUrl">The return URL.</param>
    /// <param name="callbackUrl">The callback URL.</param>
    /// <param name="authOnly">True for authorization only; otherwise for immediate capture.</param>
    /// <param name="tokenize">True to tokenize customer card; otherwise false.</param>
    /// <param name="language">The desired language of the hosted page.</param>
    /// <param name="customerPhone">The customer phone number.</param>
    /// <param name="customerEmail">The customer email address.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    public Task<HostedPagePaymentInitResponse> InitiateHostedPagePaymentAsync(
        decimal amount,
        Uri returnUrl,
        Uri callbackUrl,
        bool authOnly,
        bool tokenize,
        string language,
        string customerPhone,
        string? customerEmail,
        string description,
        Dictionary<string, string> metadata)
    {
        if (amount.GetDecimalPlaces() > 2)
        {
            throw new ArgumentException("Amount must be in SAR and have a maximum of 2 decimal places.", nameof(amount));
        }

        ArgumentNullException.ThrowIfNull(returnUrl, nameof(returnUrl));
        ArgumentNullException.ThrowIfNull(callbackUrl, nameof(callbackUrl));
        ArgumentException.ThrowIfNullOrWhiteSpace(language, nameof(language));

        var script = $$"""
            Moyasar.init({
                element: '.payment-form',
                language: '{{language}}',
                publishable_api_key: '{{_config.PublishableKey}}',
                amount: {{(int)(amount * 100)}},
                currency: 'SAR',
                methods: ['creditcard'],
                credit_card: { save_card: {{JsonSerializer.Serialize(tokenize)}}, manual: {{JsonSerializer.Serialize(authOnly)}} },
                description: {{JsonSerializer.Serialize(description)}},
                metadata: {{JsonSerializer.Serialize(metadata)}},
                callback_url: '{{returnUrl}}',
                on_completed: '{{callbackUrl}}',
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
    /// <param name="amount">The payment amount.</param>
    /// <param name="token">The source token.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> CreatePaymentAsync(PaymentSourceType paymentType, decimal amount, string token, string description, Dictionary<string, string>? metadata = null)
    {
        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            PaymentsEndpoint,
            MoyasarPaymentRequest.Create(paymentType, amount, true, token, description, metadata));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Authorizes a payment with the given details.
    /// </summary>
    /// <param name="paymentType">The payment source type.</param>
    /// <param name="amount">The authorize amount.</param>
    /// <param name="token">The source token.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> AuthorizePaymentAsync(PaymentSourceType paymentType, decimal amount, string token, string description, Dictionary<string, string>? metadata = null)
    {
        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            PaymentsEndpoint,
            MoyasarPaymentRequest.Create(paymentType, amount, false, token, description, metadata));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Captures the specified amount from the authorized amount of the specified payment.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="amount">The amount to capture.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> CapturePaymentAsync(string paymentId, decimal amount, string description, Dictionary<string, string>? metadata)
    {
        await UpdatePaymentMetadataAsync(paymentId, description, metadata);

        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            Format(CapturePaymentEndpoint, paymentId),
            MoyasarCaptureRequest.Create(amount));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Voids an authorized payment.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="amount">Not used for this provider</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> VoidPaymentAsync(string paymentId, decimal amount, string description, Dictionary<string, string>? metadata)
    {
        await UpdatePaymentMetadataAsync(paymentId, description, metadata);

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
    /// <param name="amount">The amount to refund.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> RefundPaymentAsync(string paymentId, decimal amount, string description, Dictionary<string, string>? metadata)
    {
        await UpdatePaymentMetadataAsync(paymentId, description, metadata);

        var response = await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Post,
            Format(RefundPaymentEndpoint, paymentId),
            MoyasarRefundRequest.Create(amount));

        return response!.ToGeneric();
    }

    /// <summary>
    /// Attempts to void a payment, if it's not possible, it refunds it.
    /// </summary>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="amount">The amount to void or refund.</param>
    /// <param name="description">The payment description.</param>
    /// <param name="metadata">The payment metadata.</param>
    /// <returns></returns>
    public async Task<PaymentResponse> VoidOrRefundPaymentAsync(string paymentId, decimal amount, string description, Dictionary<string, string>? metadata)
    {
        try
        {
            return await VoidPaymentAsync(paymentId, amount, description, metadata);
        }
        catch (MoyasarException)
        {
            return await RefundPaymentAsync(paymentId, amount, description, metadata);
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
    public async Task<object?> FetchPaymentAsync(string paymentId)
        => await SendRequestAsync<MoyasarPaymentResponse>(
            HttpMethod.Get,
            Format(FetchPaymentEndpoint, paymentId),
            null);

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
