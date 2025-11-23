using Peers.Core.Payments;
using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarTokenResponseTests
{
    [Fact]
    public void ToGeneric_ShouldReturnCorrectTokenResponse()
    {
        // Arrange
        var tokenResponse = new MoyasarTokenResponse
        {
            Brand = "Visa",
            Funding = "Credit",
            LastFour = "1234",
            Month = "12",
            Year = "2025",
        };

        // Act
        var result = tokenResponse.ToGeneric();

        // Assert
        Assert.Equal(PaymentCardBrand.Visa, result.CardBrand);
        Assert.Equal(PaymentCardFunding.Credit, result.CardType);
        Assert.Equal("1234", result.MaskedCardNumber);
        Assert.Equal(12, result.ExpiryMonth);
        Assert.Equal(2025, result.ExpiryYear);
    }
}
