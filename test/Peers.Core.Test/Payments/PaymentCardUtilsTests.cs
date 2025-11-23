using Peers.Core.Payments;

namespace Peers.Core.Test.Payments;

public class PaymentCardUtilsTests
{
    [Fact]
    public void GetExpiryDate_ValidDate_ReturnsExpected()
    {
        // Arrange
        var expiryYear = 2025;
        var expiryMonth = 12;
        var expectedDate = new DateOnly(2025, 12, 31);

        // Act
        var result = PaymentCardUtils.GetExpiryDate(expiryYear, expiryMonth);

        // Assert
        Assert.Equal(expectedDate, result);
    }

    [Fact]
    public void IsExpired_NullDate_ReturnsTrue()
    {
        // Arrange
        DateOnly? expiryDate = null;
        var currentDate = new DateOnly(2024, 1, 1);

        // Act
        var result = PaymentCardUtils.IsExpired(expiryDate, currentDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExpired_ExpiredDate_ReturnsTrue()
    {
        // Arrange
        var expiryDate = new DateOnly(2023, 1, 1);
        var currentDate = new DateOnly(2024, 1, 1);

        // Act
        var result = PaymentCardUtils.IsExpired(expiryDate, currentDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExpired_ValidDate_ReturnsFalse()
    {
        // Arrange
        var expiryDate = new DateOnly(2025, 1, 1);
        var currentDate = new DateOnly(2024, 1, 1);

        // Act
        var result = PaymentCardUtils.IsExpired(expiryDate, currentDate);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("VISA", PaymentCardBrand.Visa)]
    [InlineData("MASTERCARD", PaymentCardBrand.MasterCard)]
    [InlineData("AMEX", PaymentCardBrand.Amex)]
    [InlineData("MADA", PaymentCardBrand.Mada)]
    [InlineData("visa", PaymentCardBrand.Visa)]
    [InlineData("mastercard", PaymentCardBrand.MasterCard)]
    [InlineData("amex", PaymentCardBrand.Amex)]
    [InlineData("mada", PaymentCardBrand.Mada)]
    [InlineData("Visa", PaymentCardBrand.Visa)]
    [InlineData("MasterCard", PaymentCardBrand.MasterCard)]
    [InlineData("Amex", PaymentCardBrand.Amex)]
    [InlineData("Mada", PaymentCardBrand.Mada)]
    [InlineData("master", PaymentCardBrand.MasterCard)]
    [InlineData("MAster", PaymentCardBrand.MasterCard)]
    [InlineData("MAster_Card", PaymentCardBrand.MasterCard)]
    [InlineData("amerICANexpress", PaymentCardBrand.Amex)]
    [InlineData("amerICAN_express", PaymentCardBrand.Amex)]
    public void ResolveCardBrand_ValidValues_ReturnsExpected(string input, PaymentCardBrand expected)
    {
        // Act
        var result = PaymentCardUtils.ResolveCardBrand(input);
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ResolveCardBrand_InvalidValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = "INVALID";
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentCardUtils.ResolveCardBrand(invalidValue));
    }
    [Theory]
    [InlineData("CREDIT", PaymentCardFunding.Credit)]
    [InlineData("DEBIT", PaymentCardFunding.Debit)]
    [InlineData("credit", PaymentCardFunding.Credit)]
    [InlineData("debit", PaymentCardFunding.Debit)]
    [InlineData("Credit", PaymentCardFunding.Credit)]
    [InlineData("Debit", PaymentCardFunding.Debit)]
    public void ResolveCardFunding_ValidValues_ReturnsExpected(string input, PaymentCardFunding expected)
    {
        // Act
        var result = PaymentCardUtils.ResolveCardFunding(input);
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ResolveCardFunding_InvalidValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = "INVALID";
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentCardUtils.ResolveCardFunding(invalidValue));
    }
}
