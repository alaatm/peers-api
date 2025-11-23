using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarCaptureRequestTests
{
    [Fact]
    public void Create_with_amount_with_more_than_two_decimal_places__throws_ArgumentException()
    {
        // Arrange
        var amount = 1.234m;

        // Act
        var exception = Record.Exception(() => MoyasarCaptureRequest.Create(amount));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
        Assert.Equal("Amount must be in SAR and have a maximum of 2 decimal places. (Parameter 'amount')", exception.Message);
    }

    [Fact]
    public void Create_with_amount_with_two_decimal_places__returns_CaptureRequest()
    {
        // Arrange
        var amount = 1.23m;

        // Act
        var captureRequest = MoyasarCaptureRequest.Create(amount);

        // Assert
        Assert.NotNull(captureRequest);
        Assert.Equal(123, captureRequest.Amount);
    }
}
