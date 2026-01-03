using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.ClickPay.Models;
using Peers.Core.Payments.Providers.Moyasar;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Core.Security.Jwt;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Ordering.Domain;

namespace Peers.Api.Test.EndToEnd;

[Collection("Api App Factory collection")]
public class PayPageTests
{
    private readonly ApiAppFactory _factory;
    private readonly HttpClient _client;

    public PayPageTests(ApiAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    #region Get
    #region Tokenization
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_no_sid_calls_paymentProvider_initTokenization(bool useForwardHeaders)
    {
        // Arrange
        var lang = "it";
        var initiator = "initiator";
        var returnUri = new Uri($"http://localhost/payments/result?ik={initiator}&bti=XXX");
        var callbackUri = new Uri($"http://localhost/payments/pay?ik={initiator}&bti=XXX");
        var customerPhone = "+966511111111";
        var customer = _factory.CreateCustomer("john_doe", customerPhone);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        if (useForwardHeaders)
        {
            _client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
            _client.DefaultRequestHeaders.Add("X-Forwarded-Host", "example.com");
        }

        _factory.PaymentProviderMoq.SetupGet(p => p.ProviderName).Returns(initiator);
        _factory.PaymentProviderMoq
            .Setup(p => p.InitiateHostedPageTokenizationAsync(
                It.Is<Uri>(u => MatchUri(returnUri, u, useForwardHeaders)),
                It.Is<Uri>(u => MatchUri(callbackUri, u, useForwardHeaders)),
                It.Is<PaymentInfo>(pi => pi.OrderId == $"{customer.Id}" && pi.CustomerPhone == customerPhone && pi.CustomerEmail == "user@peers.com.sa"),
                lang))
            .ReturnsAsync(new HostedPagePaymentInitResponse
            {
                Script = "script",
            });

        // Act
        var page = await _client.GetAsync($"/payments/pay?culture={lang}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        _factory.PaymentProviderMoq.VerifyAll();
    }
    #endregion

    #region Payment
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, null)]
    [InlineData(false, null)]
    public async Task Get_with_sid_calls_paymentProvider_initPayment_when_active_sid_exists(bool useForwardHeaders, bool? saveCard)
    {
        // Arrange
        var lang = "it";
        var initiator = "initiator";
        var checkoutSession = _factory.CreateCheckoutSession();
        var customer = checkoutSession.Cart.Buyer;
        var sid = checkoutSession.SessionId;
        var returnUri = new Uri($"http://localhost/payments/result?ik={initiator}&bti=XXX&sid={sid:N}");
        var callbackUri = new Uri($"http://localhost/payments/pay?ik={initiator}&bti=XXX&sid={sid:N}");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        if (useForwardHeaders)
        {
            _client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
            _client.DefaultRequestHeaders.Add("X-Forwarded-Host", "example.com");
        }

        _factory.PaymentProviderMoq.SetupGet(p => p.ProviderName).Returns(initiator);
        _factory.PaymentProviderMoq
            .Setup(p => p.InitiateHostedPagePaymentAsync(
                It.Is<Uri>(u => MatchUri(returnUri, u, useForwardHeaders)),
                It.Is<Uri>(u => MatchUri(callbackUri, u, useForwardHeaders)),
                It.Is<PaymentInfo>(pi => pi.OrderId == $"{sid}" && pi.CustomerPhone == customer.User.PhoneNumber && pi.CustomerEmail == "user@peers.com.sa"),
                true,
                saveCard ?? false,
                lang))
            .ReturnsAsync(new HostedPagePaymentInitResponse
            {
                Script = "script",
            });

        var url = saveCard.HasValue
            ? $"/payments/pay?culture={lang}&sid={sid:N}&saveCard={saveCard.Value}"
            : $"/payments/pay?culture={lang}&sid={sid:N}";

        // Act
        var page = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_with_invalid_sid_redirects_to_result_page_with_failure(bool nonGuid)
    {
        // Arrange
        var sid = nonGuid ? "123" : Guid.NewGuid().ToString();
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        _factory.PaymentProviderMoq.SetupGet(p => p.ProviderName).Returns("initiator");

        // Act
        var page = await _client.GetAsync($"/payments/pay?culture=en&sid={sid}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);

        var locationHeader = page.Headers.Location.ToString();
        Assert.StartsWith($"/Payments/Result?{TokenIdResolver.TokenIdQueryKey}=", locationHeader);
        Assert.EndsWith("&paymentProcessFailed=True", locationHeader);
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_redirects_to_result_page_with_failure_when_passing_invalid_or_nonExisting_sessionId(bool isInvalid)
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        _factory.PaymentProviderMoq.SetupGet(p => p.ProviderName).Returns(string.Empty);

        // Act
        var page = await _client.GetAsync($"/payments/pay?culture=en&sid={(isInvalid ? "123" : Guid.NewGuid())}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);

        var locationHeader = page.Headers.Location.ToString();
        Assert.StartsWith($"/Payments/Result?{TokenIdResolver.TokenIdQueryKey}=", locationHeader);
        Assert.EndsWith("&paymentProcessFailed=True", locationHeader);
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_with_sid_not_beloning_to_authenticated_customer_redirects_to_result_page_with_failure()
    {
        // Arrange
        var checkoutSession = _factory.CreateCheckoutSession();
        var sid = checkoutSession.SessionId;

        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        _factory.PaymentProviderMoq.SetupGet(p => p.ProviderName).Returns("initiator");

        // Act
        var page = await _client.GetAsync($"/payments/pay?culture=en&sid={sid}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);

        var locationHeader = page.Headers.Location.ToString();
        Assert.StartsWith($"/Payments/Result?{TokenIdResolver.TokenIdQueryKey}=", locationHeader);
        Assert.EndsWith("&paymentProcessFailed=True", locationHeader);
        _factory.PaymentProviderMoq.VerifyAll();
    }
    #endregion

    #region Shared
    [Fact]
    public async Task Get_renders_page_with_script_when_init_returns_script()
    {
        // Arrange
        var scriptResult = "the script";
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        SetupPaymentProvider(new HostedPagePaymentInitResponse
        {
            Script = scriptResult,
        });

        // Act
        var page = await _client.GetAsync("/payments/pay?culture=en");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var content = await HtmlHelpers.GetDocumentAsync(page);

        var cssLink = content.QuerySelector("link[rel='stylesheet'][href='https://cdn.jsdelivr.net/npm/moyasar-payment-form@2.2.3/dist/moyasar.css']");
        Assert.NotNull(cssLink);

        var jsScript = content.QuerySelector("script[src='https://cdn.jsdelivr.net/npm/moyasar-payment-form@2.2.3/dist/moyasar.umd.min.js']");
        Assert.NotNull(jsScript);

        var paymentFormDiv = content.QuerySelector("div.payment-form");
        Assert.NotNull(paymentFormDiv);

        var inlineScript = content.QuerySelector("body > script:not([src])");
        Assert.NotNull(inlineScript);
        Assert.Contains(scriptResult, inlineScript.TextContent.Trim());

        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_redirects_when_init_returns_redirect()
    {
        // Arrange
        var redirectUrl = new Uri("https://example.com");
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        SetupPaymentProvider(new HostedPagePaymentInitResponse
        {
            RedirectUrl = redirectUrl,
        });

        // Act
        var page = await _client.GetAsync("/payments/pay?culture=en");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);
        Assert.Equal(redirectUrl.ToString(), page.Headers.Location.ToString());
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_redirects_to_result_page_with_failure_on_unexpected_init_response()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        SetupPaymentProvider(new HostedPagePaymentInitResponse());

        // Act
        var page = await _client.GetAsync("/payments/pay?culture=en");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);

        var locationHeader = page.Headers.Location.ToString();
        Assert.StartsWith($"/Payments/Result?{TokenIdResolver.TokenIdQueryKey}=", locationHeader);
        Assert.EndsWith("&paymentProcessFailed=True", locationHeader);
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_redirects_to_result_page_with_failure_when_init_throws()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(customer.Id, customer.Username));
        SetupPaymentProvider(null);

        // Act
        var page = await _client.GetAsync("/payments/pay?culture=en");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);

        var locationHeader = page.Headers.Location.ToString();
        Assert.StartsWith($"/Payments/Result?{TokenIdResolver.TokenIdQueryKey}=", locationHeader);
        Assert.EndsWith("&paymentProcessFailed=True", locationHeader);
        _factory.PaymentProviderMoq.VerifyAll();
    }
    #endregion
    #endregion

    #region Post
    [Fact]
    public async Task Post_returns_BadRequest_on_unknown_initiator()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse();

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik=UNKN&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    [Fact]
    public async Task Post_returns_BadRequest_on_missing_initiator()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse();

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    [Fact]
    public async Task Post_returns_BadRequest_on_invalid_moyasar_response()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse(hasId: false);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={MoyasarPaymentProvider.Name}&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    [Fact]
    public async Task Post_returns_BadRequest_on_invalid_clickpay_response()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var clickpayResponse = CreateClickPayCallbackResponse(hasId: false);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={ClickPayPaymentProvider.Name}&bti={tokenId}", clickpayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    [Fact]
    public async Task Post_does_not_create_paymentCard_on_failed_moyasar_callback_response()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse(successfull: false);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={MoyasarPaymentProvider.Name}&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    [Fact]
    public async Task Post_does_not_create_paymentCard_on_successfull_moyasar_callback_response_with_no_token()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse(hasToken: false);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={MoyasarPaymentProvider.Name}&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    [Fact]
    public async Task Post_does_not_create_paymentCard_on_failed_clickpay_callback_response()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var clickPayResponse = CreateClickPayCallbackResponse(successfull: false);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(clickPayResponse.TranRef, null))
            .Returns(Task.CompletedTask);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={ClickPayPaymentProvider.Name}&bti={tokenId}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
        _factory.PaymentProcessorMoq.VerifyAll();
    }

    [Fact]
    public async Task Post_does_not_create_paymentCard_on_successfull_clickpay_callback_response_with_no_token()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var clickPayResponse = CreateClickPayCallbackResponse(hasToken: false);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(clickPayResponse.TranRef, null))
            .Returns(Task.CompletedTask);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={ClickPayPaymentProvider.Name}&bti={tokenId}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
        _factory.PaymentProcessorMoq.VerifyAll();
    }

    [Fact]
    public async Task Post_creates_and_adds_paymentCard_on_successfull_moyasar_callback_with_token()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse();

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={MoyasarPaymentProvider.Name}&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        var tokenizedCard = Assert.Single(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card) as PaymentCard;
        Assert.False(tokenizedCard.IsVerified);
        Assert.Equal(moyasarResponse.Id, tokenizedCard.PaymentId);
        Assert.Equal("466666", tokenizedCard.First6Digits);
        Assert.Equal("8888", tokenizedCard.Last4Digits);
        Assert.Equal(moyasarResponse.Source.Token, tokenizedCard.Token);
    }

    [Fact]
    public async Task Post_creates_and_adds_paymentCard_on_successfull_clickpay_callback_response_with_token()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var clickPayResponse = CreateClickPayCallbackResponse(true);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(clickPayResponse.TranRef, null))
            .Returns(Task.CompletedTask);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={ClickPayPaymentProvider.Name}&bti={tokenId}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        var tokenizedCard = Assert.Single(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card) as PaymentCard;
        Assert.False(tokenizedCard.IsVerified);
        Assert.Equal(clickPayResponse.TranRef, tokenizedCard.PaymentId);
        Assert.Equal(PaymentCardBrand.Mada, tokenizedCard.Brand);
        Assert.Equal(PaymentCardFunding.Debit, tokenizedCard.Funding);
        Assert.Equal(new DateOnly(2025, 12, 31), tokenizedCard.Expiry);
        Assert.Equal("466666", tokenizedCard.First6Digits);
        Assert.Equal("8888", tokenizedCard.Last4Digits);
        Assert.Equal(clickPayResponse.Token, tokenizedCard.Token);
        _factory.PaymentProcessorMoq.VerifyAll();
    }

    #region Payment
    [Fact]
    public async Task Post_for_moyasar_marks_session_as_payInProgress_if_a_valid_and_existing_session_is_provided()
    {
        // Arrange
        var checkoutSession = _factory.CreateCheckoutSession();
        var customer = checkoutSession.Cart.Buyer;
        var sid = checkoutSession.SessionId;
        var moyasarResponse = CreateMoyasarCallbackResponse(hasToken: false, sessionId: sid.ToString("N"));
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={MoyasarPaymentProvider.Name}&bti={tokenId}&sid={sid:N}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var sessionFromDb = _factory.GetSession(checkoutSession.Id);
        Assert.Equal(CheckoutSessionStatus.Paying, sessionFromDb.Status);
        Assert.Equal(moyasarResponse.Id, sessionFromDb.PaymentId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_for_moyasar_does_not_mark_session_as_payInProgress_if_a_invalid_or_nonExisting_session_is_provided(bool isValidSession)
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var sid = isValidSession ? Guid.NewGuid().ToString("N") : "123";
        var moyasarResponse = CreateMoyasarCallbackResponse(hasToken: false, sessionId: sid);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={MoyasarPaymentProvider.Name}&bti={tokenId}&sid={sid}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
    }

    [Fact]
    public async Task Post_for_moyasar_does_not_mark_session_as_payInProgress_if_a_session_does_not_belong_to_authenticated_customer()
    {
        // Arrange
        var checkoutSession = _factory.CreateCheckoutSession();
        var sid = checkoutSession.SessionId;

        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse(hasToken: false, sessionId: sid.ToString("N"));

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={MoyasarPaymentProvider.Name}&bti={tokenId}&sid={sid:N}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var sessionFromDb = _factory.GetSession(checkoutSession.Id);

        Assert.Equal(CheckoutSessionStatus.IntentIssued, sessionFromDb.Status);
        Assert.Null(sessionFromDb.PaymentId);
    }

    // /////////

    [Fact]
    public async Task Post_for_clickpay_mark_session_as_payInProgress_if_a_valid_and_existing_session_is_provided()
    {
        // Arrange
        var checkoutSession = _factory.CreateCheckoutSession();
        var customer = checkoutSession.Cart.Buyer;
        var sid = checkoutSession.SessionId;
        var clickPayResponse = CreateClickPayCallbackResponse(hasToken: false, sessionId: sid.ToString("N"));
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(clickPayResponse.TranRef, clickPayResponse.CartId))
            .Returns(Task.CompletedTask);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={ClickPayPaymentProvider.Name}&bti={tokenId}&sid={sid:N}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var sessionFromDb = _factory.GetSession(checkoutSession.Id);
        Assert.Equal(CheckoutSessionStatus.Paying, sessionFromDb.Status);
        Assert.Equal(clickPayResponse.TranRef, sessionFromDb.PaymentId);
    }

    [Fact]
    public async Task Post_for_clickpay_calls_paymentProcessor_if_a_valid_and_existing_session_is_provided()
    {
        // Arrange
        var checkoutSession = _factory.CreateCheckoutSession();
        var customer = checkoutSession.Cart.Buyer;
        var sid = checkoutSession.SessionId;
        var clickPayResponse = CreateClickPayCallbackResponse(hasToken: false, sessionId: sid.ToString("N"));
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(clickPayResponse.TranRef, clickPayResponse.CartId))
            .Returns(Task.CompletedTask);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={ClickPayPaymentProvider.Name}&bti={tokenId}&sid={sid:N}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        _factory.PaymentProcessorMoq.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_for_clickpay_does_not_mark_session_as_payInProgress_if_a_invalid_or_nonExisting_session_is_provided(bool isValidSession)
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var sid = isValidSession ? Guid.NewGuid().ToString("N") : "123";
        var clickPayResponse = CreateClickPayCallbackResponse(hasToken: false, sessionId: sid);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(clickPayResponse.TranRef, clickPayResponse.CartId))
            .Returns(Task.CompletedTask);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={ClickPayPaymentProvider.Name}&bti={tokenId}&sid={sid}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        _factory.PaymentProcessorMoq.VerifyAll();
    }

    [Fact]
    public async Task Post_for_clickpay_does_not_mark_session_as_payInProgress_if_a_session_does_not_belong_to_authenticated_customer()
    {
        // Arrange
        var checkoutSession = _factory.CreateCheckoutSession();
        var sid = checkoutSession.SessionId;

        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var clickPayResponse = CreateClickPayCallbackResponse(hasToken: false, sessionId: sid.ToString("N"));
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(clickPayResponse.TranRef, clickPayResponse.CartId))
            .Returns(Task.CompletedTask);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/pay?culture=en&ik={ClickPayPaymentProvider.Name}&bti={tokenId}&sid={sid:N}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var sessionFromDb = _factory.GetSession(checkoutSession.Id);
        Assert.Equal(CheckoutSessionStatus.IntentIssued, sessionFromDb.Status);
        Assert.Null(sessionFromDb.PaymentId);
        _factory.PaymentProcessorMoq.VerifyAll();
    }
    #endregion
    #endregion

    private static MoyasarPaymentResponse CreateMoyasarCallbackResponse(
        bool hasId = true,
        bool hasToken = true,
        bool successfull = true,
        string sessionId = null)
    {
        var response = new MoyasarPaymentResponse()
        {
            Id = hasId ? Guid.NewGuid().ToString() : null,
            Status = successfull ? MoyasarPaymentResponse.StatusInitiated : MoyasarPaymentResponse.StatusFailed,
            Source = new MoyasarPaymentResponseSource
            {
                Number = "4666-66XX-XXXX-8888",
                Token = hasToken ? Guid.NewGuid().ToString() : null,
            },
        };

        if (sessionId is not null)
        {
            response.Metadata = new Dictionary<string, string>
            {
                { PaymentInfo.OrderIdKey, sessionId }
            };
        }

        return response;
    }

    private static ClickPayHostedPageCallbackResponse CreateClickPayCallbackResponse(
        bool hasId = true,
        bool hasToken = true,
        bool successfull = true,
        string sessionId = null) => new()
        {
            TranRef = hasId ? Guid.NewGuid().ToString() : null,
            Token = hasToken ? Guid.NewGuid().ToString() : null,
            CartId = sessionId,
            PaymentResult = new ClickPayPaymentResult
            {
                ResponseStatus = successfull ? "A" : "E",
            },
            PaymentInfo = new ClickPayPaymentInfo
            {
                CardScheme = "Mada",
                CardType = "Debit",
                PaymentDescription = "4666 66## #### 8888",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
            },
        };

    private void SetupPaymentProvider(HostedPagePaymentInitResponse initResponse)
    {
        _factory.PaymentProviderMoq.SetupGet(p => p.ProviderName).Returns(string.Empty);
        var setup = _factory.PaymentProviderMoq
            .Setup(p => p.InitiateHostedPageTokenizationAsync(
                It.IsAny<Uri>(),
                It.IsAny<Uri>(),
                It.IsAny<PaymentInfo>(),
                It.IsAny<string>()));

        if (initResponse is null)
        {
            setup.ThrowsAsync(new MoyasarException("Error"));
        }
        else
        {
            setup.ReturnsAsync(initResponse);
        }
    }

    private static bool MatchUri(Uri left, Uri right, bool useForwardHeaders)
    {
        var leftQuery = QueryHelpers.ParseQuery(left.Query);
        leftQuery.Remove("bti");
        var rightQuery = QueryHelpers.ParseQuery(right.Query);
        var hasBti = rightQuery.Remove("bti", out var actualBti);

        var scheme = useForwardHeaders ? "https" : right.Scheme;
        var host = useForwardHeaders ? "example.com" : right.Host;

        return
            scheme == right.Scheme &&
            host == right.Host &&
            left.LocalPath == right.LocalPath &&
            leftQuery.Count == rightQuery.Count &&
            leftQuery.All(kvp =>
                rightQuery.TryGetValue(kvp.Key, out var value) &&
                kvp.Value.Count == value.Count &&
                kvp.Value.All(v => v == value[0])
            ) &&
            hasBti &&
            actualBti.ToString().Length == 32;
    }
}
