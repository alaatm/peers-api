using System.Globalization;
using System.Net;
using System.Text.Json;
using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Core.Payments.Providers.ClickPay.Models;
using RichardSzalay.MockHttp;

namespace Peers.Core.Test.Payments.Providers.ClickPay;

public class ClickPayPaymentProviderTests
{
    private static readonly ClickPayConfig _config = new()
    {
        ProfileId = "profile_test",
        Key = "test",
        PayoutAccountId = "test"
    };

    [Fact]
    public void ProviderName_is_clickpay()
    {
        // Arrange
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);

        // Act
        var name = provider.ProviderName;

        // Assert
        Assert.Equal("clickpay", name);
    }

    #region InitiateHostedPageTokenizationAsync
    [Fact]
    public async Task InitiateHostedPageTokenizationAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForTransactionApi(11, Guid.NewGuid().ToString(), "description");

        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.InitiateHostedPageTokenizationAsync(null, null, info1, null));
        Assert.Equal("PaymentInfo intent must be Tokenization for hosted page tokenization requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.InitiateHostedPageTokenizationAsync(null, null, info2, null));
        Assert.Equal("PaymentInfo intent must be Tokenization for hosted page tokenization requests.", ex.Message);
    }

    [Fact]
    public async Task InitiateHostedPageTokenizationAsync_calls_hotedPageEndpoint_and_returns_redirectUrl_when_success()
    {
        // Arrange
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var info = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var language = "en";

        var request = ClickPayHostedPagePaymentRequest.Create(_config.ProfileId, language, true, true, returnUrl, callbackUrl, info);
        var paymentResponse = new ClickPayHostedPagePaymentResponse { RedirectUrl = new Uri("https://example.com/redirect") };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            request,
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPageTokenizationAsync(returnUrl, callbackUrl, info, language);

        // Assert
        Assert.Equal("https://example.com/redirect", response.RedirectUrl.ToString());
    }
    #endregion

    #region InitiateHostedPagePaymentAsync
    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(5, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForTransactionApi(11, Guid.NewGuid().ToString(), "description");

        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);

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
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(null, default, PaymentInfo.ForHpp(1, "a", "a", "a", "a"), default, default, default));
        Assert.Equal("returnUrl", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_callbackUrl_is_null()
    {
        // Arrange
        var url = new Uri("https://example.com");
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(url, null, PaymentInfo.ForHpp(1, "a", "a", "a", "a"), default, default, default));
        Assert.Equal("callbackUrl", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_paymentInfo_is_null()
    {
        // Arrange
        var url = new Uri("https://example.com");
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
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
        var exType = lang == null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var url = new Uri("https://example.com");
        var info = PaymentInfo.ForHpp(1, "a", "a", "a", "a");
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync(exType, () => provider.InitiateHostedPagePaymentAsync(url, url, info, default, default, lang));
        Assert.Contains("language", ex.Message);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_calls_hotedPageEndpoint_and_returns_redirectUrl_when_success()
    {
        // Arrange
        var language = "en";
        var info = PaymentInfo.ForHpp(12, Guid.NewGuid().ToString(), "test description", "+966511111111", "test@example.com", new Dictionary<string, string>
        {
            { "booking", "123" },
            { "k2", "v2" },
        });
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var authOnly = true;
        var tokenize = false;

        var request = ClickPayHostedPagePaymentRequest.Create(_config.ProfileId, language, authOnly, tokenize, returnUrl, callbackUrl, info);
        var paymentResponse = new ClickPayHostedPagePaymentResponse { RedirectUrl = new Uri("https://example.com/redirect") };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            request,
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPagePaymentAsync(returnUrl, callbackUrl, info, authOnly, tokenize, language);

        // Assert
        Assert.Equal("https://example.com/redirect", response.RedirectUrl.ToString());
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_returns_default_genericResponse_when_hotedPageEndpoint_returns_null()
    {
        // Arrange
        var language = "en";
        var info = PaymentInfo.ForHpp(12, Guid.NewGuid().ToString(), "test description", "+966511111111", "test@example.com", new Dictionary<string, string>
        {
            { "booking", "123" },
            { "k2", "v2" },
        });
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var authOnly = true;
        var tokenize = false;

        var request = ClickPayHostedPagePaymentRequest.Create(_config.ProfileId, language, authOnly, tokenize, returnUrl, callbackUrl, info);
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            request,
            HttpStatusCode.OK, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPagePaymentAsync(returnUrl, callbackUrl, info, authOnly, tokenize, language);

        // Assert
        Assert.Null(response.RedirectUrl);
        Assert.Null(response.Script);
    }
    #endregion

    #region CreatePaymentAsync
    [Fact]
    public async Task CreatePaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");

        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.CreatePaymentAsync(default, "token", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.CreatePaymentAsync(default, "token", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);
    }

    [Fact]
    public async Task CreatePaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusPaid, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("sale", token, null, amount, orderId, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CreatePaymentAsync(default, token, info);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Payment, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task CreatePaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("sale", token, null, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.CreatePaymentAsync(default, token, info));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.Trace, ex.ErrorObject.Trace);
    }

    [Fact]
    public async Task CreatePaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 12;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("sale", token, null, amount, orderId, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.CreatePaymentAsync(default, token, info));
    }
    #endregion

    #region AuthorizePaymentAsync
    [Fact]
    public async Task AuthorizePaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");

        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);

        // Act & assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.AuthorizePaymentAsync(default, "token", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);

        ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.AuthorizePaymentAsync(default, "token", info1));
        Assert.Equal("PaymentInfo intent must be TransactionApi for transaction API payment requests.", ex.Message);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusAuth, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("auth", token, null, amount, orderId, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.AuthorizePaymentAsync(default, token, info);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Authorization, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("auth", token, null, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.AuthorizePaymentAsync(default, token, info));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.Trace, ex.ErrorObject.Trace);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 12;
        var orderId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("auth", token, null, amount, orderId, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.AuthorizePaymentAsync(default, token, info));
    }
    #endregion

    #region CapturePaymentAsync
    [Fact]
    public async Task CapturePaymentAsync_throws_on_invalid_paymentIntent()
    {
        // Arrange
        var info1 = PaymentInfo.ForTokenization(1, "1234567890", "info@example.com");
        var info2 = PaymentInfo.ForHpp(11, Guid.NewGuid().ToString(), "description", "1234567890", "info@example.com");

        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);

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
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusCapture, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("capture", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CapturePaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Capture, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task CapturePaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var amount = 56.45m;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("capture", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.CapturePaymentAsync(paymentId, info));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.Trace, ex.ErrorObject.Trace);
    }

    [Fact]
    public async Task CapturePaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var amount = 12;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("capture", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

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

        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);

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
        var amount = 12.54m;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusVoid, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Void, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task VoidPaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var amount = 45m;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.VoidPaymentAsync(paymentId, info));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.Trace, ex.ErrorObject.Trace);
    }

    [Fact]
    public async Task VoidPaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var amount = 45m;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

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

        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);

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
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusRefund, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.RefundPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Refund, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task RefundPaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var amount = 98;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.RefundPaymentAsync(paymentId, info));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.Trace, ex.ErrorObject.Trace);
    }

    [Fact]
    public async Task RefundPaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var amount = 98;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

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
        var url = $"https://secure.clickpay.com.sa/payment/query";

        var paymentResponse = BuildTestPaymentResponse("anything", 100);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("queryPayment", null, paymentId),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = (ClickPayPaymentResponse)(await provider.FetchPaymentAsync(paymentId)).ProviderSpecificResponse;

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.TranRef);
    }

    [Fact]
    public async Task FetchPaymentAsync_returns_null_when_404_is_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://secure.clickpay.com.sa/payment/query";

        var paymentResponse = BuildTestPaymentResponse("anything", 100);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("queryPayment", null, paymentId),
            HttpStatusCode.NotFound, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.FetchPaymentAsync(paymentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchPaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://secure.clickpay.com.sa/payment/query";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("queryPayment", null, paymentId),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.FetchPaymentAsync(paymentId));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.Trace, ex.ErrorObject.Trace);
    }

    [Fact]
    public async Task FetchPaymentAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var url = $"https://secure.clickpay.com.sa/payment/query";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("queryPayment", null, paymentId),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

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
        var url = $"https://secure.clickpay.com.sa/payment/token";

        var tokenResponse = BuildTestTokenResponse();

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("token", token),
            HttpStatusCode.OK, tokenResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.FetchTokenAsync(token);

        // Assert
        Assert.Equal(PaymentCardBrand.Visa, result.CardBrand);
        Assert.Equal(PaymentCardFunding.Credit, result.CardType);
        Assert.Equal(tokenResponse.PaymentInfo.PaymentDescription, result.MaskedCardNumber);
        Assert.Equal(tokenResponse.PaymentInfo.ExpiryMonth, result.ExpiryMonth);
        Assert.Equal(tokenResponse.PaymentInfo.ExpiryYear, result.ExpiryYear);
    }

    [Fact]
    public async Task FetchTokenAsync_returns_null_when_404_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://secure.clickpay.com.sa/payment/token";

        var tokenResponse = BuildTestTokenResponse();

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("token", token),
            HttpStatusCode.NotFound, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.FetchTokenAsync(token);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchTokenAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://secure.clickpay.com.sa/payment/token";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("token", token),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.FetchTokenAsync(token));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.Trace, ex.ErrorObject.Trace);
    }

    [Fact]
    public async Task FetchTokenAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://secure.clickpay.com.sa/payment/token";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("token", token),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

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
        var url = $"https://secure.clickpay.com.sa/payment/token/delete";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("token", token),
            HttpStatusCode.NoContent, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

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
        var url = $"https://secure.clickpay.com.sa/payment/token/delete";

        var tokenResponse = BuildTestTokenResponse();

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("token", token),
            HttpStatusCode.NotFound, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        await provider.DeleteTokenAsync(token);

        // Assert
        // No exception
    }

    [Fact]
    public async Task DeleteTokenAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://secure.clickpay.com.sa/payment/token/delete";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("token", token),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.DeleteTokenAsync(token));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse.Trace, ex.ErrorObject.Trace);
    }

    [Fact]
    public async Task DeleteTokenAsync_throws_HttpRequestException_without_retries_when_other_than_400_404_or_transient_errors_are_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var url = $"https://secure.clickpay.com.sa/payment/token/delete";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("token", token),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.DeleteTokenAsync(token));
    }
    #endregion

    #region VoidOrRefundPaymentAsync
    [Fact]
    public async Task VoidOrRefundPaymentAsync_voids_payment_when_void_is_successful()
    {
        // Arrange
        var amount = 100m;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusVoid, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidOrRefundPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Void, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task VoidOrRefundPaymentAsync_refunds_payment_when_void_is_not_successful()
    {
        // Arrange
        var amount = 90.43m;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusRefund, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidOrRefundPaymentAsync(paymentId, info);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Refund, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task VoidOrRefundPaymentAsync_throws_when_void_and_refund_are_not_successful()
    {
        // Arrange
        var amount = 34.3m;
        var orderId = Guid.NewGuid().ToString();
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string> { { "k", "v" } };
        var info = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse1 = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg1",
            Trace = "trace1",
        };
        var errorResponse2 = new ClickPayErrorResponse
        {
            Code = 6666,
            Message = "msg2",
            Trace = "trace2",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse1);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, orderId, description, metadata),
            HttpStatusCode.BadRequest, errorResponse2);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.VoidOrRefundPaymentAsync(paymentId, info));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(errorResponse2.Code, ex.ErrorObject.Code);
        Assert.Equal(errorResponse2.Message, ex.ErrorObject.Message);
        Assert.Equal(errorResponse2.Trace, ex.ErrorObject.Trace);
    }
    #endregion

    #region Helpers
    private static ClickPayPaymentResponse BuildTestPaymentResponse(string status, decimal amount, string id = null, bool success = true) => new()
    {
        TranRef = id ?? Guid.NewGuid().ToString(),
        TranType = status,
        CartAmount = amount.ToString(CultureInfo.InvariantCulture),
        CartCurrency = "SAR",
        PaymentResult = new ClickPayPaymentResult
        {
            ResponseCode = success ? "00" : "99",
            ResponseStatus = success ? "A" : "E",
            ResponseMessage = success ? "Success" : "Error",
            TransactionTime = DateTime.UtcNow,
        },
    };

    private static ClickPayTokenResponse BuildTestTokenResponse() => new()
    {
        PaymentInfo = new ClickPayPaymentInfo
        {
            CardScheme = "Visa",
            CardType = "Credit",
            PaymentDescription = "5123 11## #### 1111",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
        },
    };

    private static ClickPayTransactionRequest BuildTestPaymentRequest(
        string type,
        string token = null,
        string paymentId = null,
        decimal? amount = null,
        string orderId = null,
        string description = null,
        Dictionary<string, string> metadata = null)
    {
        var info = PaymentInfo.ForTransactionApi(amount ?? 0.1m, orderId ?? Guid.NewGuid().ToString(), description ?? Guid.NewGuid().ToString(), metadata);
        return type switch
        {
            "sale" => ClickPayTransactionRequest.CreateSale(_config.ProfileId, token, info),
            "auth" => ClickPayTransactionRequest.CreateAuthorization(_config.ProfileId, token, info),
            "capture" => ClickPayTransactionRequest.CreateCapture(_config.ProfileId, paymentId, info),
            "void" => ClickPayTransactionRequest.CreateVoid(_config.ProfileId, paymentId, info),
            "refund" => ClickPayTransactionRequest.CreateRefund(_config.ProfileId, paymentId, info),
            "queryPayment" => ClickPayTransactionRequest.CreateQuery(_config.ProfileId, paymentId),
            "token" => ClickPayTransactionRequest.CreateTokenQueryOrDelete(_config.ProfileId, token),
            _ => null,
        };
    }

    private static MockHttpMessageHandler GetHttpMoq(HttpMethod method, string url, object payload, HttpStatusCode resultStatus, object resultObject)
        => GetHttpMoq(null, method, url, payload, resultStatus, resultObject);

    private static MockHttpMessageHandler GetHttpMoq(MockHttpMessageHandler httpMsgHandlerMoq, HttpMethod method, string url, object payload, HttpStatusCode resultStatus, object resultObject)
    {
        var httpMoq = httpMsgHandlerMoq ?? new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        var requestMoq = httpMoq
            .Expect(method, url)
            .WithHeaders("Accept", "application/json")
            .WithHeaders("Authorization", _config.Key)
            .Respond(resultStatus, "application/json", JsonSerializer.Serialize(resultObject));

        if (payload is not null)
        {
            requestMoq
                .WithHeaders("Content-Type", "application/json; charset=utf-8")
                .WithContent(JsonSerializer.Serialize(payload));
        }

        return httpMoq;
    }

    #endregion
}
