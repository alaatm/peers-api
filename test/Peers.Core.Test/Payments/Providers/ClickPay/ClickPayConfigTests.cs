using Peers.Core.Payments.Providers.ClickPay.Configuration;

namespace Peers.Core.Test.Payments.Providers.ClickPay;

public class ClickPayConfigTests
{
    private readonly ClickPayConfigValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public void Fails_validation_when_profileId_is_missing_or_empty(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.ProfileId = value;

        // Act
        var result = _validator.Validate(null, config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("ClickPayPaymentProvider:ProfileId must not be empty.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public void Fails_validation_when_key_is_missing_or_empty(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.Key = value;

        // Act
        var result = _validator.Validate(null, config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("ClickPayPaymentProvider:Key must not be empty.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public void Fails_validation_when_payoutAccountId_is_missing_or_empty(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.PayoutAccountId = value;

        // Act
        var result = _validator.Validate(null, config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("ClickPayPaymentProvider:PayoutAccountId must not be empty.", result.FailureMessage);
    }

    [Fact]
    public void Succeeds_validation_when_key_and_callbackUrl_are_present()
    {
        // Arrange
        var config = GetValidConfig();

        // Act
        var result = _validator.Validate(null, config);

        // Assert
        Assert.True(result.Succeeded);
    }

    private static ClickPayConfig GetValidConfig() => new()
    {
        ProfileId = "test",
        Key = "test",
        PayoutAccountId = "test"
    };
}
