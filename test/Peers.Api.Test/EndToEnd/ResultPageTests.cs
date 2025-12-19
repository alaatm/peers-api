using System.Net;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Ordering.Domain;

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
    public async Task Get_sets_fail_jsGlobalVars_when_paymentProcessFailed_queryString_is_set()
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

    [Theory]
    [InlineData("failed", false)]
    [InlineData("authorized", true)]
    public async Task Get_sets_jsGlobalVars_on_moyasar_return_call(string status, bool isSuccess)
    {
        // Arrange
        var message = status == "authorized" ? "successMsg" : "failMsg";
        var paymentId = Guid.NewGuid().ToString();
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(paymentId, null))
            .Returns(Task.FromResult((Order)null));

        // Act
        var page = await _client.GetAsync($"/payments/result?id={paymentId}&status={status}&bti={tokenId}&message={message}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var content = await HtmlHelpers.GetDocumentAsync(page);
        var scriptContent = content.QuerySelector("body > script:not([src])").TextContent;
        Assert.Contains($"window.success = {isSuccess.ToString().ToLowerInvariant()}", scriptContent);
        Assert.Contains($"window.message = \"{message}\"", scriptContent);
        _factory.PaymentProcessorMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_calls_paymentProcessorHandler_when_paymentId_is_set()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out _);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(paymentId, null))
            .Returns(Task.FromResult((Order)null));

        // Act
        var page = await _client.GetAsync($"/payments/result?id={paymentId}&status=any&bti={tokenId}&message=any");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        _factory.PaymentProcessorMoq.VerifyAll();
    }

    [Fact]
    public async Task Get_calls_paymentProcessorHandler_when_paymentId_is_set_and_sessionId_is_set()
    {
        // Arrange
        var paymentId = Guid.NewGuid().ToString();
        var sessionId = Guid.NewGuid().ToString();
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out _);
        _factory.PaymentProcessorMoq
            .Setup(p => p.HandleAsync(paymentId, sessionId))
            .Returns(Task.FromResult((Order)null));

        // Act
        var page = await _client.GetAsync($"/payments/result?id={paymentId}&status=any&bti={tokenId}&message=any&sid={sessionId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        _factory.PaymentProcessorMoq.VerifyAll();
    }

    [Fact]
    public async Task Post_bypass_signature_check_when_not_a_clickPay_request()
    {
        // Arrange
        _factory.SetClickPayConfig("11111", "SGJNZ96JLG-JDMKHGRWT9-RWRK2KJNRJ");
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out _);
        var payload = new FormUrlEncodedContent(BuildClickPayReturnPayload(makeValid: false).ToDictionary());

        // Act
        var page = await _client.PostAsync($"/payments/result?bti={tokenId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(null, false)]
    [InlineData("", true)]
    public async Task Post_returns_BadRequest_on_null_or_empty_signature(string signature, bool isSet)
    {
        // Arrange
        _factory.SetClickPayConfig("11111", "SGJNZ96JLG-JDMKHGRWT9-RWRK2KJNRJ");
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out _);
        var dict = BuildClickPayReturnPayload(makeValid: true).ToDictionary();
        if (isSet)
        {
            dict["signature"] = signature;
        }
        else
        {
            dict.Remove("signature");
        }
        var payload = new FormUrlEncodedContent(dict);

        // Act
        var page = await _client.PostAsync($"/payments/result?ik=clickpay&bti={tokenId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);
    }

    [Fact]
    public async Task Post_returns_BadRequest_on_bad_signature()
    {
        // Arrange
        _factory.SetClickPayConfig("11111", "SGJNZ96JLG-JDMKHGRWT9-RWRK2KJNRJ");
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out _);
        var dict = BuildClickPayReturnPayload(makeValid: true).ToDictionary();
        dict["signature"] = "7a181a32c768621eb6966107752ee70205a01f1c4403a3d13c0ff604f591f98811111";
        var payload = new FormUrlEncodedContent(dict);

        // Act
        var page = await _client.PostAsync($"/payments/result?ik=clickpay&bti={tokenId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);
    }

    [Fact]
    public async Task Post_returns_BadRequest_on_failed_clickPaySignature_validation()
    {
        // Arrange
        _factory.SetClickPayConfig("11111", "SGJNZ96JLG-JDMKHGRWT9-RWRK2KJNRJ");
        var tokenId = _factory.GenerateAndCacheTokenId(new Customer { Username = "username" }, out _);
        var payload = new FormUrlEncodedContent(BuildClickPayReturnPayload(makeValid: false).ToDictionary());

        // Act
        var page = await _client.PostAsync($"/payments/result?ik=clickpay&bti={tokenId}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, page.StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_sets_jsGlobalVars_on_clickpay_return_call(bool isSuccess)
    {
        // Arrange
        _factory.SetClickPayConfig("11111", "SGJNZ96JLG-JDMKHGRWT9-RWRK2KJNRJ");

        var payload = BuildClickPayReturnPayload(successfull: isSuccess, makeValid: true);
        var customer = _factory.CreateCustomer("john_doe", "+966511111111");
        var tokenId = _factory.GenerateAndCacheTokenId(customer, out _);
        var formContent = new FormUrlEncodedContent(payload);

        // Act
        var page = await _client.PostAsync($"/payments/result?ik=clickpay&bti={tokenId}", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var content = await HtmlHelpers.GetDocumentAsync(page);
        var scriptContent = content.QuerySelector("body > script:not([src])").TextContent;
        Assert.Contains($"window.success = {isSuccess.ToString().ToLowerInvariant()}", scriptContent);
        Assert.Contains($"window.message = \"Authorised\"", scriptContent);
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
