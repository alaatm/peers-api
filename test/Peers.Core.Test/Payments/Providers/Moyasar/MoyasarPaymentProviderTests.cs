using System.Net;
using System.Text;
using System.Text.Json;
using RichardSzalay.MockHttp;
using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar;
using Peers.Core.Payments.Providers.Moyasar.Configuration;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Core.Payments.Providers.Moyasar.Models.Payouts;

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
    public async Task InitiateHostedPageTokenizationAsync_builds_correct_script()
    {
        // Arrange
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var language = "en";
        var phone = "1234567890";
        var email = "info@example.com";

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPageTokenizationAsync(returnUrl, callbackUrl, language, phone, email);

        // Assert
        Assert.Equal("""
            Moyasar.init({
                element: '.payment-form',
                language: 'en',
                publishable_api_key: 'pk_test',
                amount: 100,
                currency: 'SAR',
                methods: ['creditcard'],
                credit_card: { save_card: true, manual: true },
                description: "Tokenize customer card",
                metadata: {"customer":"1234567890"},
                callback_url: 'https://example.com/return',
                on_completed: 'https://example.com/callback',
            });
            """, response.Script);
    }
    #endregion

    #region InitiateHostedPagePaymentAsync
    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_amount_has_more_than_two_decimal_places()
    {
        // Arrange
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => provider.InitiateHostedPagePaymentAsync(1.234m, null!, null!, true, false, null!, null, null, null, null));
        Assert.Equal("amount", ex.ParamName);
        Assert.Equal("Amount must be in SAR and have a maximum of 2 decimal places. (Parameter 'amount')", ex.Message);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_returnUrl_is_null()
    {
        // Arrange
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(12, null!, null, true, false, null!, null, null, null, null));
        Assert.Equal("returnUrl", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_callbackUrl_is_null()
    {
        // Arrange
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(12, new Uri("https://example.com"), null!, true, false, null!, null, null, null, null));
        Assert.Equal("callbackUrl", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_language_is_null()
    {
        // Arrange
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(12, new Uri("https://example.com"), new Uri("https://example.com"), true, false, null!, null, null, null, null));
        Assert.Equal("language", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task InitiateHostedPagePaymentAsync_throws_when_language_is_empty(string language)
    {
        // Arrange
        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => provider.InitiateHostedPagePaymentAsync(12, new Uri("https://example.com"), new Uri("https://example.com"), true, false, language, null, null, null, null));
        Assert.Equal("language", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_builds_correct_script()
    {
        // Arrange
        var amount = 12;
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var authOnly = true;
        var tokenize = false;
        var language = "en";
        var description = "test description";
        var metadata = new Dictionary<string, string>
        {
            { "k1", "v1" },
            { "k2", "v2" },
        };

        var provider = new MoyasarPaymentProvider(new HttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPagePaymentAsync(amount, returnUrl, callbackUrl, authOnly, tokenize, language, null, null, description, metadata);

        // Assert
        Assert.Equal("""
            Moyasar.init({
                element: '.payment-form',
                language: 'en',
                publishable_api_key: 'pk_test',
                amount: 1200,
                currency: 'SAR',
                methods: ['creditcard'],
                credit_card: { save_card: false, manual: true },
                description: "test description",
                metadata: {"k1":"v1","k2":"v2"},
                callback_url: 'https://example.com/return',
                on_completed: 'https://example.com/callback',
            });
            """, response.Script);
    }
    #endregion

    #region CreatePaymentAsync
    [Theory]
    [InlineData(PaymentSourceType.ApplePay)]
    [InlineData(PaymentSourceType.TokenizedCard)]
    public async Task CreatePaymentAsync_returns_paymentResponse_when_successful(PaymentSourceType paymentSourceType)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "k", "v" },
        };
        var url = "https://api.moyasar.com/v1/payments";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusPaid, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, true, token, amount, description, metadata),
            HttpStatusCode.Created, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CreatePaymentAsync(paymentSourceType, amount, token, description, metadata);

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
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "k", "v" },
        };
        var url = "https://api.moyasar.com/v1/payments";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, true, token, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.CreatePaymentAsync(paymentSourceType, amount, token, description, metadata));
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
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "k", "v" },
        };
        var url = "https://api.moyasar.com/v1/payments";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, true, token, amount, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.CreatePaymentAsync(paymentSourceType, amount, token, description, metadata));
    }
    #endregion

    #region AuthorizePaymentAsync
    [Theory]
    [InlineData(PaymentSourceType.ApplePay)]
    [InlineData(PaymentSourceType.TokenizedCard)]
    public async Task AuthorizePaymentAsync_returns_paymentResponse_when_successful(PaymentSourceType paymentSourceType)
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "k", "v" },
        };
        var url = "https://api.moyasar.com/v1/payments";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusAuth, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, false, token, amount, description, metadata),
            HttpStatusCode.Created, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.AuthorizePaymentAsync(paymentSourceType, amount, token, description, metadata);

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
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "k", "v" },
        };
        var url = "https://api.moyasar.com/v1/payments";

        var errorResponse = new MoyasarErrorResponse
        {
            Type = "type",
            Message = "msg",
            ErrorsEncoded = "obj",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, false, token, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.AuthorizePaymentAsync(paymentSourceType, amount, token, description, metadata));
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
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "k", "v" },
        };
        var url = "https://api.moyasar.com/v1/payments";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest(paymentSourceType, false, token, amount, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.AuthorizePaymentAsync(paymentSourceType, amount, token, description, metadata));
    }
    #endregion

    #region CapturePaymentAsync
    [Fact]
    public async Task CapturePaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var amount = 12;
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/capture";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusCapture, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestCaptureRequest(amount),
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CapturePaymentAsync(paymentId, amount, null, null);

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
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/capture";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusCapture, amount);

        var httpMoq = SetupPaymentMetadataUpdateCall(null, paymentId, true);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            BuildTestCaptureRequest(amount),
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CapturePaymentAsync(paymentId, amount, "desc", new Dictionary<string, string> { { "k", "v" } });

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
        var paymentId = Guid.NewGuid().ToString();
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
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.CapturePaymentAsync(paymentId, amount, null, null));
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
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/capture";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestCaptureRequest(amount),
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.CapturePaymentAsync(paymentId, amount, null, null));
    }
    #endregion

    #region VoidPaymentAsync
    [Fact]
    public async Task VoidPaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusVoid, 0);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidPaymentAsync(paymentId, 0, null, null);

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
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusVoid, 0);

        var httpMoq = SetupPaymentMetadataUpdateCall(null, paymentId, true);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            null,
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidPaymentAsync(paymentId, 0, "desc", new Dictionary<string, string> { { "k", "v" } });

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
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.VoidPaymentAsync(paymentId, 0, null, null));
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
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.VoidPaymentAsync(paymentId, 0, null, null));
    }
    #endregion

    #region RefundPaymentAsync
    [Fact]
    public async Task RefundPaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var amount = 98;
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusRefund, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestRefundRequest(amount),
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.RefundPaymentAsync(paymentId, amount, null, null);

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
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusRefund, amount);

        var httpMoq = SetupPaymentMetadataUpdateCall(null, paymentId, true);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            BuildTestRefundRequest(amount),
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.RefundPaymentAsync(paymentId, amount, "desc", new Dictionary<string, string> { { "k", "v" } });

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
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.RefundPaymentAsync(paymentId, 1, null, null));
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
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/refund";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.Unauthorized, null);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.RefundPaymentAsync(paymentId, 1, null, null));
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
        var url = $"https://api.moyasar.com/v1/payments/{paymentId}/void";

        var paymentResponse = BuildTestPaymentResponse(MoyasarPaymentResponse.StatusVoid, 100);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            null,
            HttpStatusCode.OK, paymentResponse);

        var provider = new MoyasarPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidOrRefundPaymentAsync(paymentId, 0, null, null);

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
        var result = await provider.VoidOrRefundPaymentAsync(paymentId, 100, null, null);

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
        var ex = await Assert.ThrowsAsync<MoyasarException>(() => provider.VoidOrRefundPaymentAsync(paymentId, 1, null, null));
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

    private static MoyasarPaymentRequest BuildTestPaymentRequest(PaymentSourceType sourceType, bool immediateCapture, string token, decimal amount, string description, Dictionary<string, string> metadata) => new()
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
