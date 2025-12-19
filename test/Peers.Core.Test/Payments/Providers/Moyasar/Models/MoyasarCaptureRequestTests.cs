using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarCaptureRequestTests
{
    [Fact]
    public void Create_returns_CaptureRequest()
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
