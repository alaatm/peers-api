using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Modules.Customers.Domain;

namespace Peers.Api.Test.EndToEnd;

[Collection("Api App Factory collection")]
public class ResultPageTests
{
    private readonly ApiAppFactory _factory;
    private readonly HttpClient _client;

    public ResultPageTests(ApiAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_removes_cached_token()
    {
        // Arrange
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out var cacheKey);

        // Act
        var page = await _client.GetAsync($"/payments/result?paymentProcessFailed=true&bti={tokenId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var cache = _factory.Services.GetRequiredService<IMemoryCache>();
        var cacheEntry = cache.Get(cacheKey);
        Assert.Null(cacheEntry);
    }

    [Fact]
    public async Task Get_sets_failureCode_when_paymentProcessFailed_queryString_is_set()
    {
        // Arrange
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out _);

        // Act
        var page = await _client.GetAsync($"/payments/result?paymentProcessFailed=true&bti={tokenId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var content = await HtmlHelpers.GetDocumentAsync(page);
        var scriptContent = content.QuerySelector("body > script:not([src])").TextContent;
        Assert.Contains("window.success = false", scriptContent);
        Assert.Contains("window.message = \"Internal Server Error\"", scriptContent);
    }

    [Fact]
    public async Task Get_on_successfull_moyasarCall_voids_tokenization_payment_and_fetches_and_updates_card_token_and_sets_jsGlobalVars()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var cardToken = Guid.NewGuid().ToString();
        var customer = _factory.CreateCustomerWithTokenizedCard("john_doe", "+966511111111", paymentId, cardToken);
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);

        _factory.PaymentProviderMoq.Setup(p => p.VoidPaymentAsync(paymentId, 1, "Void tokenization payment", null)).ReturnsAsync((PaymentResponse)null);
        _factory.PaymentProviderMoq.Setup(p => p.FetchTokenAsync(cardToken)).ReturnsAsync(new TokenResponse
        {
            CardBrand = PaymentCardBrand.Mada,
            CardType = PaymentCardFunding.Debit,
            ExpiryMonth = 12,
            ExpiryYear = 2025,
        });

        // Act
        var page = await _client.GetAsync($"/payments/result?id={paymentId}&status=authorized&bti={tokenId}&message=successMsg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var customerFromDb = _factory.GetCustomer(customer.Id);
        var tokenizedCard = Assert.Single(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card) as PaymentCard;
        Assert.Equal(PaymentCardBrand.Mada, tokenizedCard.Brand);
        Assert.Equal(PaymentCardFunding.Debit, tokenizedCard.Funding);
        Assert.Equal(new DateOnly(2025, 12, 31), tokenizedCard.Expiry);
        Assert.True(tokenizedCard.IsVerified);
        _factory.PaymentProviderMoq.VerifyAll();

        var content = await HtmlHelpers.GetDocumentAsync(page);
        var scriptContent = content.QuerySelector("body > script:not([src])").TextContent;
        Assert.Contains("window.success = true", scriptContent);
        Assert.Contains("window.message = \"successMsg\"", scriptContent);
    }

    [Fact]
    public async Task Get_on_failedMoyasarCall_deletes_saved_card_token_and_sets_jsGlobalVars()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var cardToken = Guid.NewGuid().ToString();
        var customer = _factory.CreateCustomerWithTokenizedCard("john_doe", "+966511111111", paymentId, cardToken);
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);

        // Act
        var page = await _client.GetAsync($"/payments/result?id={paymentId}&status=fail&bti={tokenId}&message=failMsg");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var customerFromDb = _factory.GetCustomer(customer.Id);
        Assert.DoesNotContain(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card);
        _factory.PaymentProviderMoq.VerifyAll();

        var content = await HtmlHelpers.GetDocumentAsync(page);
        var scriptContent = content.QuerySelector("body > script:not([src])").TextContent;
        Assert.Contains("window.success = false", scriptContent);
        Assert.Contains("window.message = \"failMsg\"", scriptContent);
    }

    [Fact]
    public async Task Post_returns_BadRequest_on_failed_clickPaySignature_validation()
    {
        // Arrange
        _factory.SetClickPayConfig("11111", "SGJNZ96JLG-JDMKHGRWT9-RWRK2KJNRJ");
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out _);
        var payload = new FormUrlEncodedContent(BuildClickPayReturnPayload(makeValid: false).ToDictionary());

        // Act
        var page = await _client.PostAsync($"/payments/result?initiator=clickpay&bti={tokenId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);
    }

    [Fact]
    public async Task Post_removes_cached_token()
    {
        // Arrange
        _factory.SetClickPayConfig("11111", "SGJNZ96JLG-JDMKHGRWT9-RWRK2KJNRJ");
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out var cacheKey);
        var payload = new FormUrlEncodedContent(BuildClickPayReturnPayload(successfull: false, makeValid: true).ToDictionary());

        // Act
        var page = await _client.PostAsync($"/payments/result?initiator=clickpay&bti={tokenId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var cache = _factory.Services.GetRequiredService<IMemoryCache>();
        var cacheEntry = cache.Get(cacheKey);
        Assert.Null(cacheEntry);
    }

    [Fact]
    public async Task Post_on_successfull_clickPayCall_voids_tokenization_payment_and_sets_jsGlobalVars()
    {
        // Arrange
        _factory.SetClickPayConfig("11111", "SGJNZ96JLG-JDMKHGRWT9-RWRK2KJNRJ");

        var payload = BuildClickPayReturnPayload(successfull: true, makeValid: true);
        var paymentId = payload["tranRef"];
        var cardToken = "";
        var customer = _factory.CreateCustomerWithTokenizedCard("john_doe", "+966511111111", paymentId, cardToken, hasMetadata: true);
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var formContent = new FormUrlEncodedContent(payload);

        _factory.PaymentProviderMoq.Setup(p => p.VoidPaymentAsync(paymentId, 1, "Void tokenization payment", null)).ReturnsAsync((PaymentResponse)null);

        // Act
        var page = await _client.PostAsync($"/payments/result?initiator=clickpay&bti={tokenId}", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var customerFromDb = _factory.GetCustomer(customer.Id);
        var tokenizedCard = Assert.Single(customerFromDb.PaymentMethods, p => p.Type is PaymentType.Card) as PaymentCard;
        Assert.True(tokenizedCard.IsVerified);
        _factory.PaymentProviderMoq.VerifyAll();

        var content = await HtmlHelpers.GetDocumentAsync(page);
        var scriptContent = content.QuerySelector("body > script:not([src])").TextContent;
        Assert.Contains("window.success = true", scriptContent);
        Assert.Contains("window.message = \"Authorised\"", scriptContent);
    }

    private static Dictionary<string, string> BuildClickPayReturnPayload(bool successfull = true, bool makeValid = true) => new()
    {
        ["acquirerMessage"] = makeValid ? "" : "x",
        ["acquirerRRN"] = "",
        ["cartId"] = "cart_11111",
        ["customerEmail"] = "email@domain.com",
        ["respCode"] = "G84718",
        ["respMessage"] = "Authorised",
        ["respStatus"] = successfull ? "A" : "D",
        ["token"] = "",
        ["tranRef"] = "TST2215201242166",
        ["signature"] = successfull ? "7a181a32c768621eb6966107752ee70205a01f1c4403a3d13c0ff604f591f988" : "8c31d64351f3164af6dff794e55cb6245ced569719fa2cd362fd1ea2016dd342",
    };
}
