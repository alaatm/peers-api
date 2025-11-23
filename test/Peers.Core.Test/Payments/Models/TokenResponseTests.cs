using Peers.Core.Payments.Models;

namespace Peers.Core.Test.Payments.Models;

public class TokenResponseTests
{
    [Fact]
    public void Expiry_Date_ShouldReturnCorrectDate()
    {
        // Arrange
        var tokenResponse = new TokenResponse
        {
            ExpiryMonth = 12,
            ExpiryYear = 2025
        };

        // Act
        var result = tokenResponse.Expiry;

        // Assert
        var expectedDate = new DateOnly(2025, 12, 31);
        Assert.Equal(expectedDate, result);
    }
}
