using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarRefundRequestTests
{
    [Fact]
    public void Create_with_amount_with_more_than_two_decimal_places__throws_ArgumentException()
    {
        // Arrange
        var amount = 1.234m;

        // Act
        var exception = Record.Exception(() => MoyasarRefundRequest.Create(amount));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
        Assert.Equal("Amount must be in SAR and have a maximum of 2 decimal places. (Parameter 'amount')", exception.Message);
    }

    [Fact]
    public void Create_with_amount_with_two_decimal_places__returns_RefundRequest()
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
