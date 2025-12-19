using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarRefundRequestTests
{
    [Fact]
    public void Create_returns_RefundRequest()
    {
        // Arrange
        var amount = 1.23m;

        // Act
        var refundRequest = MoyasarRefundRequest.Create(amount);

        // Assert
        Assert.NotNull(refundRequest);
        Assert.Equal(123, refundRequest.Amount);
    }
}
