using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Test.Payments.Providers.ClickPay.Models;

public class ClickPayErrorResponseTests
{
    [Fact]
    public void ToString_returns_string_representation_of_error_response()
    {
        // Arrange
        var errorResponse = new ClickPayErrorResponse
        {
            Code = 555,
            Message = "error message",
            Trace = "test trace",
        };

        // Act
        var result = errorResponse.ToString();

        // Assert
        Assert.Equal("Code: 555\nMessage: error message\nTrace: test trace", result);
    }
}
