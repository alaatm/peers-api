using System.Globalization;
using System.Net;
using System.Text.Json;
using RichardSzalay.MockHttp;
using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Core.Payments.Providers.ClickPay.Models;

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
    public async Task InitiateHostedPageTokenizationAsync_calls_hotedPageEndpoint_and_returns_redirectUrl_when_success()
    {
        // Arrange
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var language = "en";
        var phone = "1234567890";
        var email = "info@example.com";

        var request = ClickPayHostedPagePaymentRequest.Create(_config.ProfileId, "Tokenize customer card", language, 1, true, true, phone, email, returnUrl, callbackUrl, new Dictionary<string, string>
        {
            ["customer"] = phone,
        });
        var paymentResponse = new ClickPayHostedPagePaymentResponse { RedirectUrl = new Uri("https://example.com/redirect") };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            request,
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPageTokenizationAsync(returnUrl, callbackUrl, language, phone, email);

        // Assert
        Assert.Equal("https://example.com/redirect", response.RedirectUrl.ToString());
    }
    #endregion

    #region InitiateHostedPagePaymentAsync
    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_amount_has_more_than_two_decimal_places()
    {
        // Arrange
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => provider.InitiateHostedPagePaymentAsync(1.234m, null!, null!, true, false, null!, null, null, null, null));
        Assert.Equal("amount", ex.ParamName);
        Assert.Equal("Amount must be in SAR and have a maximum of 2 decimal places. (Parameter 'amount')", ex.Message);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_returnUrl_is_null()
    {
        // Arrange
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(12, null!, null, true, false, null!, null, null, null, null));
        Assert.Equal("returnUrl", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_callbackUrl_is_null()
    {
        // Arrange
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitiateHostedPagePaymentAsync(12, new Uri("https://example.com"), null!, true, false, null!, null, null, null, null));
        Assert.Equal("callbackUrl", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_throws_when_language_is_null()
    {
        // Arrange
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
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
        var provider = new ClickPayPaymentProvider(new HttpClient(), _config);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => provider.InitiateHostedPagePaymentAsync(12, new Uri("https://example.com"), new Uri("https://example.com"), true, false, language, null, null, null, null));
        Assert.Equal("language", ex.ParamName);
    }

    [Fact]
    public async Task InitiateHostedPagePaymentAsync_calls_hotedPageEndpoint_and_returns_redirectUrl_when_success()
    {
        // Arrange
        var amount = 12;
        var customerEmail = "info@example.com";
        var customerPhone = "+966511111111";
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var authOnly = true;
        var tokenize = false;
        var language = "en";
        var description = "test description";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "123" },
            { "k2", "v2" },
        };

        var request = ClickPayHostedPagePaymentRequest.Create(_config.ProfileId, description, language, amount, authOnly, tokenize, customerPhone, customerEmail, returnUrl, callbackUrl, metadata);
        var paymentResponse = new ClickPayHostedPagePaymentResponse { RedirectUrl = new Uri("https://example.com/redirect") };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            request,
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var response = await provider.InitiateHostedPagePaymentAsync(amount, returnUrl, callbackUrl, authOnly, tokenize, language, customerPhone, customerEmail, description, metadata);

        // Assert
        Assert.Equal("https://example.com/redirect", response.RedirectUrl.ToString());
    }
    #endregion

    #region CreatePaymentAsync
    [Fact]
    public async Task CreatePaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusPaid, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("sale", token, null, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CreatePaymentAsync(default, amount, token, description, metadata);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Payment, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task CreatePaymentAsync_throws_ClickPayException_when_non_success_respStatus_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusPaid, amount, success: false);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("sale", token, null, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.CreatePaymentAsync(default, amount, token, description, metadata));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(99, ex.ErrorObject.Code);
        Assert.Equal("E: Error", ex.ErrorObject.Message);
    }

    [Fact]
    public async Task CreatePaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("sale", token, null, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.CreatePaymentAsync(default, amount, token, description, metadata));
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
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("sale", token, null, amount, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.CreatePaymentAsync(default, amount, token, description, metadata));
    }
    #endregion

    #region AuthorizePaymentAsync
    [Fact]
    public async Task AuthorizePaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusAuth, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("auth", token, null, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.AuthorizePaymentAsync(default, amount, token, description, metadata);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Authorization, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_throws_ClickPayException_when_non_success_respStatus_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusPaid, amount, success: false);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("auth", token, null, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.AuthorizePaymentAsync(default, amount, token, description, metadata));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(99, ex.ErrorObject.Code);
        Assert.Equal("E: Error", ex.ErrorObject.Message);
    }

    [Fact]
    public async Task AuthorizePaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var token = Guid.NewGuid().ToString();
        var amount = 56.45m;
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("auth", token, null, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.AuthorizePaymentAsync(default, amount, token, description, metadata));
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
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("auth", token, null, amount, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.AuthorizePaymentAsync(default, amount, token, description, metadata));
    }
    #endregion

    #region CapturePaymentAsync
    [Fact]
    public async Task CapturePaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var amount = 12;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusCapture, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("capture", null, paymentId, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.CapturePaymentAsync(paymentId, amount, description, metadata);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Capture, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task CapturePaymentAsync_throws_ClickPayException_when_non_success_respStatus_is_returned()
    {
        // Arrange
        var amount = 56.45m;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusPaid, amount, success: false);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("capture", null, paymentId, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.CapturePaymentAsync(paymentId, amount, description, metadata));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(99, ex.ErrorObject.Code);
        Assert.Equal("E: Error", ex.ErrorObject.Message);
    }

    [Fact]
    public async Task CapturePaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var amount = 56.45m;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("capture", null, paymentId, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.CapturePaymentAsync(paymentId, amount, description, metadata));
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
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("capture", null, paymentId, amount, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.CapturePaymentAsync(paymentId, amount, description, metadata));
    }
    #endregion

    #region VoidPaymentAsync
    [Fact]
    public async Task VoidPaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var amount = 12.54m;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusVoid, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidPaymentAsync(paymentId, amount, description, metadata);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Void, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task VoidPaymentAsync_throws_ClickPayException_when_non_success_respStatus_is_returned()
    {
        // Arrange
        var amount = 56.45m;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusPaid, amount, success: false);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.VoidPaymentAsync(paymentId, amount, description, metadata));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(99, ex.ErrorObject.Code);
        Assert.Equal("E: Error", ex.ErrorObject.Message);
    }

    [Fact]
    public async Task VoidPaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var amount = 45m;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.VoidPaymentAsync(paymentId, amount, description, metadata));
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
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.VoidPaymentAsync(paymentId, amount, description, metadata));
    }
    #endregion

    #region RefundPaymentAsync
    [Fact]
    public async Task RefundPaymentAsync_returns_paymentResponse_when_successful()
    {
        // Arrange
        var amount = 98;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusRefund, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.RefundPaymentAsync(paymentId, amount, description, metadata);

        // Assert
        Assert.Equal(paymentResponse.TranRef, result.PaymentId);
        Assert.Equal(PaymentOperationType.Refund, result.Operation);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(paymentResponse.CartCurrency, result.Currency);
        Assert.Equal(paymentResponse.PaymentResult.TransactionTime, result.Timestamp);
    }

    [Fact]
    public async Task RefundPaymentAsync_throws_ClickPayException_when_non_success_respStatus_is_returned()
    {
        // Arrange
        var amount = 56.45m;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = "https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusPaid, amount, success: false);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.RefundPaymentAsync(paymentId, amount, description, metadata));
        Assert.Equal("ClickPay API call failed.", ex.Message);
        Assert.Equal(99, ex.ErrorObject.Code);
        Assert.Equal("E: Error", ex.ErrorObject.Message);
    }

    [Fact]
    public async Task RefundPaymentAsync_throws_ClickPayException_when_400_is_returned()
    {
        // Arrange
        var amount = 98;
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.RefundPaymentAsync(paymentId, amount, description, metadata));
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
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, description, metadata),
            HttpStatusCode.Unauthorized, null);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => provider.RefundPaymentAsync(paymentId, amount, description, metadata));
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
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusVoid, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidOrRefundPaymentAsync(paymentId, amount, description, metadata);

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
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
        var url = $"https://secure.clickpay.com.sa/payment/request";

        var errorResponse = new ClickPayErrorResponse
        {
            Code = 5555,
            Message = "msg",
            Trace = "trace",
        };

        var paymentResponse = BuildTestPaymentResponse(ClickPayPaymentResponse.StatusRefund, amount);

        var httpMoq = GetHttpMoq(HttpMethod.Post, url,
            BuildTestPaymentRequest("void", null, paymentId, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, description, metadata),
            HttpStatusCode.OK, paymentResponse);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act
        var result = await provider.VoidOrRefundPaymentAsync(paymentId, amount, description, metadata);

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
        var paymentId = Guid.NewGuid().ToString();
        var description = "test";
        var metadata = new Dictionary<string, string>
        {
            { "booking", "v" },
        };
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
            BuildTestPaymentRequest("void", null, paymentId, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse1);
        GetHttpMoq(httpMoq, HttpMethod.Post, url,
            BuildTestPaymentRequest("refund", null, paymentId, amount, description, metadata),
            HttpStatusCode.BadRequest, errorResponse2);

        var provider = new ClickPayPaymentProvider(httpMoq.ToHttpClient(), _config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ClickPayException>(() => provider.VoidOrRefundPaymentAsync(paymentId, amount, description, metadata));
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
        string description = null,
        Dictionary<string, string> metadata = null)
        => type switch
        {
            "sale" => ClickPayTransactionRequest.CreateSale(_config.ProfileId, amount.Value, token, description, metadata),
            "auth" => ClickPayTransactionRequest.CreateAuthorization(_config.ProfileId, amount.Value, token, description, metadata),
            "capture" => ClickPayTransactionRequest.CreateCapture(_config.ProfileId, paymentId, amount.Value, description, metadata),
            "void" => ClickPayTransactionRequest.CreateVoid(_config.ProfileId, paymentId, amount.Value, description, metadata),
            "refund" => ClickPayTransactionRequest.CreateRefund(_config.ProfileId, paymentId, amount.Value, description, metadata),
            "queryPayment" => ClickPayTransactionRequest.CreateQuery(_config.ProfileId, paymentId),
            "token" => ClickPayTransactionRequest.CreateTokenQueryOrDelete(_config.ProfileId, token),
            _ => null,
        };

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
