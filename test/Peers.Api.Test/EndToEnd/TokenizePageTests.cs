using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar;
using Peers.Core.Payments.Providers.Moyasar.Models;
using Peers.Modules.Customers.Domain;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.ClickPay.Models;
using Peers.Core.Payments;

namespace Peers.Api.Test.EndToEnd;

[Collection("Api App Factory collection")]
public class TokenizePageTests
{
    private readonly ApiAppFactory _factory;
    private readonly HttpClient _client;

    public TokenizePageTests(ApiAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_calls_paymentProvider_init(bool useForwardHeaders)
    {
        // Arrange
        var lang = "it";
        var initiator = "initiator";
        var returnUri = new Uri($"http://localhost/payments/result?initiator={initiator}&bti=XXX");
        var callbackUri = new Uri($"http://localhost/payments/tokenize?initiator={initiator}&bti=XXX");
        var userId = 888;
        var username = "john_doe";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(userId, username));
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
                lang, username, ""))
            .ReturnsAsync(new HostedPagePaymentInitResponse
            {
                Script = "script",
            });

        // Act
        var page = await _client.GetAsync($"/payments/tokenize?culture={lang}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_renders_page_with_script_when_init_returns_script()
    {
        // Arrange
        var scriptResult = "the script";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(1, "username"));
        SetupPaymentProvider(new HostedPagePaymentInitResponse
        {
            Script = scriptResult,
        });

        // Act
        var page = await _client.GetAsync("/payments/tokenize?culture=en");

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
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(1, "username"));
        SetupPaymentProvider(new HostedPagePaymentInitResponse
        {
            RedirectUrl = redirectUrl,
        });

        // Act
        var page = await _client.GetAsync("/payments/tokenize?culture=en");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);
        Assert.Equal(redirectUrl.ToString(), page.Headers.Location.ToString());
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_redirects_to_result_page_with_failure_on_unexpected_init_response()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(1, "username"));
        SetupPaymentProvider(new HostedPagePaymentInitResponse());

        // Act
        var page = await _client.GetAsync("/payments/tokenize?culture=en");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);

        var locationHeader = page.Headers.Location.ToString();
        Assert.StartsWith("/Payments/Result?tokenId=", locationHeader);
        Assert.EndsWith("&paymentProcessFailed=True", locationHeader);
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_redirects_to_result_page_with_failure_when_init_throws()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateBearerToken(1, "username"));
        SetupPaymentProvider(null);

        // Act
        var page = await _client.GetAsync("/payments/tokenize?culture=en");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);

        var locationHeader = page.Headers.Location.ToString();
        Assert.StartsWith("/Payments/Result?tokenId=", locationHeader);
        Assert.EndsWith("&paymentProcessFailed=True", locationHeader);
        _factory.PaymentProviderMoq.VerifyAll();
    }

    [Fact]
    public async Task Post_creates_and_adds_paymentCard_on_successfull_moyasar_callback()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse();

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/tokenize?culture=en&initiator={MoyasarPaymentProvider.Name}&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.Created, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        var tokenizedCard = Assert.Single(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card) as PaymentCard;
        Assert.False(tokenizedCard.IsVerified);
        Assert.Equal(moyasarResponse.Id, tokenizedCard.PaymentId);
        Assert.Equal("466666", tokenizedCard.First6Digits);
        Assert.Equal("8888", tokenizedCard.Last4Digits);
        Assert.Equal(moyasarResponse.Source.Token, tokenizedCard.Token);
    }

    [Theory]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, false)]
    public async Task Post_returns_BadRequest_on_invalid_moyasar_callback(bool hasId, bool hasSource, bool hasNumber, bool hasToken)
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse(hasId, hasSource, hasNumber, hasToken);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/tokenize?culture=en&initiator={MoyasarPaymentProvider.Name}&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    [Fact]
    public async Task Post_creates_and_adds_paymentCard_on_successfull_clickpay_callback_response()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var clickPayResponse = CreateClickPayCallbackResponse(true);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/tokenize?culture=en&initiator={ClickPayPaymentProvider.Name}&bti={tokenId}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.Created, page.StatusCode);

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
    }

    [Fact]
    public async Task Post_does_not_create_paymentCard_on_failed_clickpay_callback_response()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var clickPayResponse = CreateClickPayCallbackResponse(false);

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/tokenize?culture=en&initiator={ClickPayPaymentProvider.Name}&bti={tokenId}", clickPayResponse);

        // Assert
        Assert.Equal(HttpStatusCode.Created, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    [Fact]
    public async Task Post_returns_BadRequest_on_unknown_initiator()
    {
        // Arrange
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var moyasarResponse = CreateMoyasarCallbackResponse();

        // Act
        var page = await _client.PostAsJsonAsync($"/payments/tokenize?culture=en&initiator=UNKN&bti={tokenId}", moyasarResponse);

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
        var page = await _client.PostAsJsonAsync($"/payments/tokenize?culture=en&bti={tokenId}", moyasarResponse);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);

        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
    }

    private static MoyasarPaymentResponse CreateMoyasarCallbackResponse(
        bool hasId = true,
        bool hasSource = true,
        bool hasNumber = true,
        bool hasToken = true) => new()
        {
            Id = hasId ? Guid.NewGuid().ToString() : null,
            Source = hasSource
                ? new MoyasarPaymentResponseSource
                {
                    Number = hasNumber ? "4666-66XX-XXXX-8888" : null,
                    Token = hasToken ? Guid.NewGuid().ToString() : null,
                }
                : null,
        };

    private static ClickPayHostedPageCallbackResponse CreateClickPayCallbackResponse(bool successfull) => new()
    {
        TranRef = Guid.NewGuid().ToString(),
        Token = Guid.NewGuid().ToString(),
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
                It.IsAny<string>(),
                It.IsAny<string>(),
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
