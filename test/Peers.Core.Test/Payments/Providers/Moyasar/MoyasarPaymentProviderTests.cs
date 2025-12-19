using System.Net;
using System.Text;
using System.Text.Json;
using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar;
using Peers.Core.Payments.Providers.Moyasar.Configuration;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Core.Payments.Providers.Moyasar.Models.Payouts;
using RichardSzalay.MockHttp;

namespace Peers.Core.Test.Payments.Providers.Moyasar;

public class MoyasarPaymentProviderTests
{
    private static readonly MoyasarConfig _config = new()
    {
        PublishableKey = "pk_test",
        Key = "test",
        PayoutAccountId = "payout_test"
    };

    [Fact]
    public void ProviderName_is_moyasar()
    {
        // Arrange
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act
        var name = provider.ProviderName;

        // Assert
        Assert.Equal("moyasar", name);
    }

    #region InitiateHostedPageTokenizationAsync
    [Fact]
    public async Task InitiateHostedPageTokenizationAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForTransactionApi(11, Guid.NewGuid().ToString(), "description");

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.InitiateHostedPageTokenizationAsync(null, null, info1, null));
        Assert.Equal("PaymentInfo intent must be Tokenization for hosted page tokenization requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.InitiateHostedPageTokenizationAsync(null, null, info2, null));
        Assert.Equal("PaymentInfo intent must be Tokenization for hosted page tokenization requests.", ex.Message);
    }

    [Fact]
    public async Task InitiateHostedPageTokenizationAsync_builds_correct_script()
    {
        // Arrange
        var language = "en";
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var info = PaymentInfo.ForTokenization(555, "1234567890", "info@example.com");

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPageTokenizationAsync(returnUrl, callbackUrl, info, language);

        // Assert
        Assert.Equal($$"""
            Moyasar.init({
                element: '.payment-form',
                language: '{{language}}',
                publishable_api_key: '{{_config.PublishableKey}}',
                amount: 100,
                currency: 'SAR',
                methods: ['creditcard'],
                credit_card: { save_card: true, manual: true },
                description: "{{info.Description}}",
                metadata: {"{{PaymentInfo.OrderIdKey}}":"555"},
                callback_url: '{{returnUrl}}',
                on_completed: async function (payment) {
                    await fetch("{{callbackUrl}}", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(payment),
                    });
                },
            });
            """, response.Script);
    }
    #endregion

    #region InitiateHostedPagePaymentAsync
    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(5, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForTransactionApi(11, Guid.NewGuid().ToString(), "description");

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.InitiateHostedPagePaymentAsync(null, null, info1, default, default, null));
        Assert.Equal("PaymentInfo intent must be HostedPaymentPage for hosted page payment requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.InitiateHostedPagePaymentAsync(null, null, info2, default, default, null));
        Assert.Equal("PaymentInfo intent must be HostedPaymentPage for hosted page payment requests.", ex.Message);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_returnUrl_is_null()
    {
        // Arrange
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(null, default, PaymentInfo.ForHpp(1, "a", "a", "a", "a"), default, default, default));
        Assert.Equal("returnUrl", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_callbackUrl_is_null()
    {
        // Arrange
        var url = new Uri("https://example.com");
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(url, null, PaymentInfo.ForHpp(1, "a", "a", "a", "a"), default, default, default));
        Assert.Equal("callbackUrl", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_paymentInfo_is_null()
    {
        // Arrange
        var url = new Uri("https://example.com");
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(url, url, null, default, default, default));
        Assert.Equal("paymentInfo", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task InitiateHostedPagePaymentAsync_throws_when_language_is_null_or_empty(string lang)
    {
        // Arrange
        var exType = lang is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var url = new Uri("https://example.com");
        var info = PaymentInfo.ForHpp(1, "a", "a", "a", "a");
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync(exType, () => provider.InitiateHostedPagePaymentAsync(url, url, info, default, default, lang));
        Assert.Contains("language", ex.Message);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_builds_correct_script()
    {
        // Arrange
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var authOnly = true;
        var tokenize = false;
        var language = "en";
        var info = PaymentInfo.ForHpp(12, Guid.NewGuid().ToString(), "test description", "+966511111111", "test@example.com", new Dictionary<string, string>
        {
            { "k1", "v1" },
            { "k2", "v2" },
        });

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPagePaymentAsync(returnUrl, callbackUrl, info, authOnly, tokenize, language);

        // Assert
        Assert.Equal($$"""
            Moyasar.init({
                element: '.payment-form',
                language: '{{language}}',
                publishable_api_key: '{{_config.PublishableKey}}',
                amount: 1200,
                currency: 'SAR',
                methods: ['creditcard'],
                credit_card: { save_card: false, manual: true },
                description: "{{info.Description}}",
                metadata: {"k1":"v1","k2":"v2","{{PaymentInfo.OrderIdKey}}":"{{info.OrderId}}"},
                callback_url: '{{returnUrl}}',
                on_completed: async function (payment) {
                    await fetch("{{callbackUrl}}", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(payment),
                    });
                },
            });
            """, response.Script);
    }
    #endregion

    #region CreatePaymentAsync
    [Fact]
    public async Task CreatePaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.CreatePaymentAsync(default, "token", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.CreatePaymentAsync(default, "token", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);
    }

    [Theory]
    [InlineData(PaymentSourceType.ApplePay)]
    [InlineData(PaymentSourceType.TokenizedCard)]
    public async Task CreatePaymentAsync_returns_paymentResponse_when_successful(PaymentSourceType paymentSourceType)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://api.moyasar.com/v1/payments";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusPaid, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, true, token, amount, orderId, description, metadata),
            HttpStatusCode.Created, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CreatePaymentAsync(paymentSourceType, token, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Payment, result.Operation);
        Assert.Equal(paymentResponse.Amount, result.Amount * 100);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.CreatedAt, result.Timestamp);
    }

    [Theory]
    [InlineData(PaymentSourceType.ApplePay)]
    [InlineData(PaymentSourceType.TokenizedCard)]
    public async Task CreatePaymentAsync_throws_MoyasarException_when_400_is_returned(PaymentSourceType paymentSourceType)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://api.moyasar.com/v1/payments";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, true, token, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.CreatePaymentAsync(paymentSourceType, token, info));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Theory]
    [InlineData(PaymentSourceType.ApplePay)]
    [InlineData(PaymentSourceType.TokenizedCard)]
    public async Task CreatePaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned(PaymentSourceType paymentSourceType)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 12;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://api.moyasar.com/v1/payments";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, true, token, amount, orderId, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.CreatePaymentAsync(paymentSourceType, token, info));
    }
    #endregion

    #region AuthorizePaymentAsync
    [Fact]
    public async Task AuthorizePaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.AuthorizePaymentAsync(default, "token", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.AuthorizePaymentAsync(default, "token", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);
    }

    [Theory]
    [InlineData(PaymentSourceType.ApplePay)]
    [InlineData(PaymentSourceType.TokenizedCard)]
    public async Task AuthorizePaymentAsync_returns_paymentResponse_when_successful(PaymentSourceType paymentSourceType)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://api.moyasar.com/v1/payments";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusAuth, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, false, token, amount, orderId, description, metadata),
            HttpStatusCode.Created, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.AuthorizePaymentAsync(paymentSourceType, token, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Authorization, result.Operation);
        Assert.Equal(paymentResponse.Amount, result.Amount * 100);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.CreatedAt, result.Timestamp);
    }

    [Theory]
    [InlineData(PaymentSourceType.ApplePay)]
    [InlineData(PaymentSourceType.TokenizedCard)]
    public async Task AuthorizePaymentAsync_throws_MoyasarException_when_400_is_returned(PaymentSourceType paymentSourceType)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://api.moyasar.com/v1/payments";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, false, token, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.AuthorizePaymentAsync(paymentSourceType, token, info));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Theory]
    [InlineData(PaymentSourceType.ApplePay)]
    [InlineData(PaymentSourceType.TokenizedCard)]
    public async Task AuthorizePaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned(PaymentSourceType paymentSourceType)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 12;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://api.moyasar.com/v1/payments";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, false, token, amount, orderId, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.AuthorizePaymentAsync(paymentSourceType, token, info));
    }
    #endregion

    #region CapturePaymentAsync
    [Fact]
    public async Task CapturePaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.CapturePaymentAsync("paymentId", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.CapturePaymentAsync("paymentId", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);
    }

    [Fact]
    public async Task CapturePaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var amount = 12;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(amount, Guid.NewGuid().ToString(), "description");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/capture";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusCapture, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestCaptureRequest(amount),
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CapturePaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Capture, result.Operation);
        Assert.Equal(paymentResponse.Captured, result.Amount * 100);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.CapturedAt, result.Timestamp);
    }

    [Fact]
    public async Task CapturePaymentAsync_updates_paymentMetadata()
    {
        // Arrange
        var amount = 12;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(amount, Guid.NewGuid().ToString(), "desc", new Dictionary<string, string> { { "k", "v" } });
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/capture";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusCapture, amount);

        var httpMoq = SetupPaymentMetadataUpdateCall(null, paymentId, true);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            BuildTestCaptureRequest(amount),
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CapturePaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Capture, result.Operation);
        Assert.Equal(paymentResponse.Captured, result.Amount * 100);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.CapturedAt, result.Timestamp);
    }

    [Fact]
    public async Task CapturePaymentAsync_throws_MoyasarException_when_400_is_returned()
    {
        // Arrange
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(amount, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/capture";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestCaptureRequest(amount),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.CapturePaymentAsync(paymentId, info));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Fact]
    public async Task CapturePaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var amount = 12;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(amount, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/capture";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestCaptureRequest(amount),
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.CapturePaymentAsync(paymentId, info));
    }
    #endregion

    #region VoidPaymentAsync
    [Fact]
    public async Task VoidPaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.VoidPaymentAsync("paymentId", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.VoidPaymentAsync("paymentId", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);
    }

    [Fact]
    public async Task VoidPaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(1, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusVoid, 0);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Void, result.Operation);
        Assert.Equal(0, result.Amount);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.VoidedAt, result.Timestamp);
    }

    [Fact]
    public async Task VoidPaymentAsync_updates_paymentMetadata()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(1, Guid.NewGuid().ToString(), "desc", new Dictionary<string, string> { { "k", "v" } });
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusVoid, 0);

        var httpMoq = SetupPaymentMetadataUpdateCall(null, paymentId, true);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            null,
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Void, result.Operation);
        Assert.Equal(0, result.Amount);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.VoidedAt, result.Timestamp);
    }

    [Fact]
    public async Task VoidPaymentAsync_throws_MoyasarException_when_400_is_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(1, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.VoidPaymentAsync(paymentId, info));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Fact]
    public async Task VoidPaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(1, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.VoidPaymentAsync(paymentId, info));
    }
    #endregion

    #region RefundPaymentAsync
    [Fact]
    public async Task RefundPaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.RefundPaymentAsync("paymentId", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.RefundPaymentAsync("paymentId", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);
    }

    [Fact]
    public async Task RefundPaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var amount = 98;
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(amount, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusRefund, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestRefundRequest(amount),
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.RefundPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Refund, result.Operation);
        Assert.Equal(paymentResponse.Refunded, result.Amount * 100);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.RefundedAt, result.Timestamp);
    }

    [Fact]
    public async Task RefundPaymentAsync_updates_paymentMetadata()
    {
        // Arrange
        var amount = 98;
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(amount, Guid.NewGuid().ToString(), "desc", new Dictionary<string, string> { { "k", "v" } });
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusRefund, amount);

        var httpMoq = SetupPaymentMetadataUpdateCall(null, paymentId, true);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            BuildTestRefundRequest(amount),
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.RefundPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Refund, result.Operation);
        Assert.Equal(paymentResponse.Refunded, result.Amount * 100);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.RefundedAt, result.Timestamp);
    }

    [Fact]
    public async Task RefundPaymentAsync_throws_MoyasarException_when_400_is_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(1, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.RefundPaymentAsync(paymentId, info));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Fact]
    public async Task RefundPaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(1, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.RefundPaymentAsync(paymentId, info));
    }
    #endregion

    #region FetchPaymentAsync
    [Fact]
    public async Task FetchPaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}";

        var paymentResponse = BuildTestPaymentResponse("anything", 100);

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = (MoyasarPaymentResponse)(await provider.FetchPaymentAsync(paymentId)).ProviderSpecificResponse;

        // Assert
        Assert.Equal(paymentResponse.Id, result.Id);
    }

    [Fact]
    public async Task FetchPaymentAsync_returns_null_when_404_is_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}";

        var paymentResponse = BuildTestPaymentResponse("anything", 100);

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.NotFound, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.FetchPaymentAsync(paymentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchPaymentAsync_throws_MoyasarException_when_400_is_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.FetchPaymentAsync(paymentId));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Fact]
    public async Task FetchPaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}";

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.FetchPaymentAsync(paymentId));
    }
    #endregion

    #region FetchTokenAsync
    [Fact]
    public async Task FetchTokenAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/tokens/{token}";

        var tokenResponse = BuildTestTokenResponse();

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.OK, tokenResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.FetchTokenAsync(token);

        // Assert
        Assert.Equal(PaymentCardBrand.Visa, result.CardBrand);
        Assert.Equal(PaymentCardFunding.Credit, result.CardType);
        Assert.Equal(tokenResponse.LastFour, result.MaskedCardNumber);
        Assert.Equal(tokenResponse.Month, $"{result.ExpiryMonth}");
        Assert.Equal(tokenResponse.Year, $"{result.ExpiryYear}");
    }

    [Fact]
    public async Task FetchTokenAsync_returns_null_when_404_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/tokens/{token}";

        var tokenResponse = BuildTestTokenResponse();

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.NotFound, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.FetchTokenAsync(token);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchTokenAsync_throws_MoyasarException_when_400_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/tokens/{token}";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.FetchTokenAsync(token));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Fact]
    public async Task FetchTokenAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/tokens/{token}";

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.FetchTokenAsync(token));
    }
    #endregion

    #region DeleteTokenAsync
    [Fact]
    public async Task DeleteTokenAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/tokens/{token}";

        var httpMoq = GetHttpMoq(HttpMethod.Delete, url,
            null,
            HttpStatusCode.NoContent, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        await provider.DeleteTokenAsync(token);

        // Assert
        // No exception
    }

    [Fact]
    public async Task DeleteTokenAsync_noop_when_404_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/tokens/{token}";

        var tokenResponse = BuildTestTokenResponse();

        var httpMoq = GetHttpMoq(HttpMethod.Delete, url,
            null,
            HttpStatusCode.NotFound, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        await provider.DeleteTokenAsync(token);

        // Assert
        // No exception
    }

    [Fact]
    public async Task DeleteTokenAsync_throws_MoyasarException_when_400_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/tokens/{token}";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Delete, url,
            null,
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.DeleteTokenAsync(token));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Fact]
    public async Task DeleteTokenAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/tokens/{token}";

        var httpMoq = GetHttpMoq(HttpMethod.Delete, url,
            null,
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.DeleteTokenAsync(token));
    }
    #endregion

    #region VoidOrRefundPaymentAsync
    [Fact]
    public async Task VoidOrRefundPaymentAsync_voids_payment_when_void_is_successful()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(1, Guid.NewGuid().ToString(), "desc");
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusVoid, 100);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidOrRefundPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Void, result.Operation);
        Assert.Equal(0, result.Amount);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.VoidedAt, result.Timestamp);
    }

    [Fact]
    public async Task VoidOrRefundPaymentAsync_refunds_payment_when_void_is_not_successful()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(100, Guid.NewGuid().ToString(), "desc");
        var voidUrl = $"https://api.moyasar.com/v1/payments/{paymentId}/void";
        var refundUrl = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusRefund, 100);

        var httpMoq = GetHttpMoq(HttpMethod.Post, voidUrl,
            null,
            HttpStatusCode.BadRequest, errorResponse);
        GetHttpMoq(httpMoq, HttpMethod.Post, refundUrl,
            null,
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidOrRefundPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.Id, result.PaymentId);
        Assert.Equal(PaymentOperationType.Refund, result.Operation);
        Assert.Equal(paymentResponse.Refunded, result.Amount * 100);
        Assert.Equal(paymentResponse.Currency, result.Currency);
        Assert.Equal(paymentResponse.RefundedAt, result.Timestamp);
    }

    [Fact]
    public async Task VoidOrRefundPaymentAsync_throws_when_void_and_refund_are_not_successful()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var info = PaymentInfo.ForTransactionApi(1, Guid.NewGuid().ToString(), "desc");
        var voidUrl = $"https://api.moyasar.com/v1/payments/{paymentId}/void";
        var refundUrl = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var errorResponse1 = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };
        var errorResponse2 = new MoyasarErrorResponse
        {
            Type = "type2",
            Message = "msg2",
            ErrorsEncoded = "obj2",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, voidUrl,
            null,
            HttpStatusCode.BadRequest, errorResponse1);
        GetHttpMoq(httpMoq, HttpMethod.Post, refundUrl,
            null,
            HttpStatusCode.BadRequest, errorResponse2);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.VoidOrRefundPaymentAsync(paymentId, info));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse2.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse2.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse2.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }
    #endregion

    #region SendPayoutsAsync
    [Fact]
    public async Task SendPayoutsAsync_returns_payoutResponse_when_successful()
    {
        // Arrange
        var url = $"https://api.moyasar.com/v1/payouts/bulk";

        var payoutRequest = BuildTestPayoutRequest();
        var payoutResponse = BuildTestPayoutResponse();

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            MoyasarPayoutRequest.Create(_config.PayoutAccountId, payoutRequest),
            HttpStatusCode.OK, payoutResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.SendPayoutsAsync(payoutRequest);

        // Assert
        Assert.Equal(payoutResponse.Entries.Length, result.Entries.Length);
        for (var i = 0; i < payoutResponse.Entries.Length; i++)
        {
            Assert.Null(result.Entries[i].BatchId);
            Assert.Equal(0, result.Entries[i].Fee);
            Assert.Equal(payoutResponse.Entries[i].UpdatedAt, result.Entries[i].Timestamp);
            Assert.Equal(payoutResponse.Entries[i].Id, result.Entries[i].EntryId);
            Assert.Equal(payoutResponse.Entries[i].Amount, result.Entries[i].Amount * 100m);
            Assert.Equal(payoutResponse.Entries[i].Amount, result.Entries[i].Total * 100m);
            Assert.Equal(payoutResponse.Entries[i].Currency, result.Entries[i].Currency);
        }
    }

    [Fact]
    public async Task SendPayoutsAsync_throws_MoyasarException_when_400_is_returned()
    {
        // Arrange
        var url = $"https://api.moyasar.com/v1/payouts/bulk";

        var payoutRequest = BuildTestPayoutRequest();
        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            MoyasarPayoutRequest.Create(_config.PayoutAccountId, payoutRequest),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.SendPayoutsAsync(payoutRequest));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Fact]
    public async Task SendPayoutsAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var url = $"https://api.moyasar.com/v1/payouts/bulk";

        var payoutRequest = BuildTestPayoutRequest();

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            MoyasarPayoutRequest.Create(_config.PayoutAccountId, payoutRequest),
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.SendPayoutsAsync(payoutRequest));
    }
    #endregion

    #region FetchPayoutAsync
    [Fact]
    public async Task FetchPayoutAsync_returns_payoutResponse_when_successful()
    {
        // Arrange
        var payoutId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payouts/{payoutId}";

        var payoutResponse = BuildTestPayoutResponse().Entries[0];

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.OK, payoutResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.FetchPayoutAsync(payoutId);

        // Assert
        Assert.Equal(payoutResponse.Id, result.EntryId);
    }

    [Fact]
    public async Task FetchPayoutAsync_returns_null_when_404_is_returned()
    {
        // Arrange
        var payoutId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payouts/{payoutId}";

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.NotFound, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.FetchPayoutAsync(payoutId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchPayoutAsync_throws_MoyasarException_when_400_is_returned()
    {
        // Arrange
        var payoutId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payouts/{payoutId}";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.FetchPayoutAsync(payoutId));
        Assert.Equal("Moyasar API call failed.", ex.Message);
        Assert.Equal(errorResponse.Type, ex.ErrorObject.Type);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.ErrorsEncoded.ToString(), ex.ErrorObject.ErrorsEncoded.ToString());
    }

    [Fact]
    public async Task FetchPayoutAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var payoutId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payouts/{payoutId}";

        var httpMoq = GetHttpMoq(HttpMethod.Get, url,
            null,
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.FetchPayoutAsync(payoutId));
    }
    #endregion

    #region Helpers
    private static MoyasarPaymentResponse BuildTestPaymentResponse(string status, decimal amount_, string id = null)
    {
        var amount = (int)(100 * (status is MoyasarPaymentResponse.StatusPaid or MoyasarPaymentResponse.StatusAuth ? amount_ : 0));
        var captured = (int)(100 * (status is MoyasarPaymentResponse.StatusCapture ? amount_ : 0));
        var refunded = (int)(100 * (status is MoyasarPaymentResponse.StatusRefund ? amount_ : 0));

        return new()
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Status = status,
            Amount = amount,
            Captured = captured,
            Refunded = refunded,
            Currency = "SAR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CapturedAt = status is MoyasarPaymentResponse.StatusCapture ? DateTime.UtcNow : null,
            VoidedAt = status is MoyasarPaymentResponse.StatusVoid ? DateTime.UtcNow : null,
            RefundedAt = status is MoyasarPaymentResponse.StatusRefund ? DateTime.UtcNow : null,
        };
    }

    private static MoyasarTokenResponse BuildTestTokenResponse() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Status = Guid.NewGuid().ToString(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Brand = "visa",
        Funding = "credit",
        LastFour = "1234",
        Month = "12",
        Year = "2025",
    };

    private static MoyasarPaymentRequest BuildTestPaymentRequest(PaymentSourceType sourceType, bool immediateCapture, string token, decimal amount, string orderId, string description, Dictionary<string, string> metadata)
    {
        metadata[PaymentInfo.OrderIdKey] = orderId;

        return new()
        {
            Amount = (int)(amount * 100),
            Currency = "SAR",
            Description = description,
            Metadata = metadata,
            Source = sourceType is PaymentSourceType.ApplePay
            ? new MoyasarApplePayPaymentSource
            {
                Manual = immediateCapture ? "false" : "true",
                Token = token,
            }
            : new MoyasarTokenPaymentSource
            {
                Manual = immediateCapture ? "false" : "true",
                Token = token,
                ThreeDSecure = false,
            },
        };
    }

    private static MoyasarCaptureRequest BuildTestCaptureRequest(decimal amount) => new()
    {
        Amount = (int)(amount * 100),
    };

    private static MoyasarRefundRequest BuildTestRefundRequest(decimal amount) => new()
    {
        Amount = (int)(amount * 100),
    };

    private static PayoutRequest BuildTestPayoutRequest() => new()
    {
        Entries =
        [
            new PayoutRequestEntry
            {
                Amount = 100,
                Bank = new BankInfo
                {
                    Iban = "SA1234567890123456789012",
                },
                Beneficiary = new BeneficiaryInfo
                {
                    Name = "Test Beneficiary",
                    PhoneNumber = "+966500000000",
                    Country = "SA",
                    City = "Riyadh",
                },
            },
        ],
    };

    private static MoyasarPayoutResponse BuildTestPayoutResponse() => new()
    {
        Entries =
        [
            new MoyasarPayoutResponseEntry
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 10000,
                Currency = "SAR",
                Status = "pending",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                UpdatedAt = DateTime.UtcNow,
                Destination = new MoyasarPayoutDestination
                {
                    Iban = "SA1234567890123456789012",
                },
            },
        ],
    };

    private static MockHttpMessageHandler GetHttpMoq(HttpMethod method, string url, object payload, HttpStatusCode resultStatus, object resultObject)
        => GetHttpMoq(null, method, url, payload, resultStatus, resultObject);

    private static MockHttpMessageHandler GetHttpMoq(MockHttpMessageHandler httpMsgHandlerMoq, HttpMethod method, string url, object payload, HttpStatusCode resultStatus, object resultObject)
    {
        var httpMoq = httpMsgHandlerMoq ?? new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        var requestMoq = httpMoq
            .Expect(method, url)
            .WithHeaders("Accept", "application/json")
            .WithHeaders("Authorization", $"Basic {GetKey()}")
            .Respond(resultStatus, "application/json", JsonSerializer.Serialize(resultObject));

        if (payload is not null)
        {
            requestMoq
                .WithHeaders("Content-Type", "application/json; charset=utf-8")
                .WithContent(JsonSerializer.Serialize(payload));
        }

        return httpMoq;
    }

    private static MockHttpMessageHandler SetupPaymentMetadataUpdateCall(MockHttpMessageHandler httpMsgHandlerMoq, string paymentId, bool successfull)
    {
        // Arrange
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}";
        object response = successfull
            ? new MoyasarPaymentResponse()
            : new MoyasarErrorResponse
            {
                Type = "type",
                Message = "msg",
                ErrorsEncoded = "obj",
            };

        var httpMoq = httpMsgHandlerMoq ?? new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        GetHttpMoq(httpMoq, HttpMethod.Put, url,
            null,
            HttpStatusCode.OK, response);

        return httpMoq;
    }

    private static string GetKey()
    {
        var keyBytes = Encoding.UTF8.GetBytes(_config.Key);
        return Convert.ToBase64String(keyBytes);
    }
    #endregion
}
