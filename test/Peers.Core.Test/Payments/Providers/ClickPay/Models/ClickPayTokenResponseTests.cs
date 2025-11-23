using Peers.Core.Payments;
using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Test.Payments.Providers.ClickPay.Models;

public class ClickPayTokenResponseTests
{
    [Fact]
    public void ToGeneric_ShouldReturnCorrectTokenResponse()
    {
        // Arrange
        var tokenResponse = new ClickPayTokenResponse
        {
            PaymentInfo = new ClickPayPaymentInfo
            {
                CardScheme = "Visa",
                CardType = "Credit",
                PaymentDescription = "5000 11## #### 1111",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
            },
        };

        // Act
        var result = tokenResponse.ToGeneric();

        // Assert
        Assert.Equal(PaymentCardBrand.Visa, result.CardBrand);
        Assert.Equal(PaymentCardFunding.Credit, result.CardType);
        Assert.Equal("5000 11## #### 1111", result.MaskedCardNumber);
        Assert.Equal(12, result.ExpiryMonth);
        Assert.Equal(2025, result.ExpiryYear);
    }
}
