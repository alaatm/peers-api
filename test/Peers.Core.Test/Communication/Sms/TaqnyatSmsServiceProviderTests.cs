using System.Net;
using System.Text.Json;
using Peers.Core.Communication.Sms;
using Peers.Core.Communication.Sms.Configuration;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;

namespace Peers.Core.Test.Communication.Sms;

public class TaqnyatSmsServiceProviderTests
{
    private static readonly SmsConfig _config = new()
    {
        Sender = "test_sender",
        Key = "test_key",
        Enabled = true,
    };

    [Fact]
    public async Task SendAsync_returns_response_when_call_is_successful()
    {
        // Arrange
        var recipient = "+966123456789";
        var body = "Test message";
        var expectedResponse = new TaqnyatResponse { MessageId = 12345 };
        var httpMoq = GetHttpMoq(
            null,
            HttpMethod.Post,
            "https://api.taqnyat.sa/v1/messages",
            new
            {
                sender = _config.Sender,
                recipients = new[] { "966123456789" },
                body,
            },
            HttpStatusCode.OK,
            expectedResponse);

        var service = new TaqnyatSmsServiceProvider(httpMoq.ToHttpClient(), _config, Mock.Of<ILogger<TaqnyatSmsServiceProvider>>());

        // Act
        var response = await service.SendAsync(recipient, body);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedResponse.MessageId, response.MessageId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SendAsync_returns_null_when_call_is_not_successful(bool hasErrorResponse)
    {
        // Arrange
        var recipient = "+966123456789";
        var body = "Test message";
        var httpMoq = GetHttpMoq(
            null,
            HttpMethod.Post,
            "https://api.taqnyat.sa/v1/messages",
            new
            {
                sender = _config.Sender,
                recipients = new[] { "966123456789" },
                body,
            },
            HttpStatusCode.Unauthorized,
            hasErrorResponse ? new TaqnyatErrorResponse() : null);

        var service = new TaqnyatSmsServiceProvider(httpMoq.ToHttpClient(), _config, Mock.Of<ILogger<TaqnyatSmsServiceProvider>>());

        // Act
        var response = await service.SendAsync(recipient, body);

        // Assert
        Assert.Null(response);
    }

    private static MockHttpMessageHandler GetHttpMoq(
        MockHttpMessageHandler httpMsgHandlerMoq,
        HttpMethod method,
        string url,
        object payload,
        HttpStatusCode resultStatus,
        object resultObject)
    {
        var httpMoq = httpMsgHandlerMoq ?? new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
        var requestMoq = httpMoq
            .Expect(method, url)
            .WithHeaders("Accept", "application/json")
            .WithHeaders("Authorization", $"Bearer {_config.Key}")
            .Respond(resultStatus, "application/json", JsonSerializer.Serialize(resultObject));

        if (payload is not null)
        {
            requestMoq
                .WithHeaders("Content-Type", "application/json; charset=utf-8")
                .WithContent(JsonSerializer.Serialize(payload));
        }

        return httpMoq;
    }
}
