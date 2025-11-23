using Peers.Core.Payments.Providers.Moyasar.Configuration;

namespace Peers.Core.Test.Payments.Providers.Moyasar;

public class MoyasarConfigTests
{
    private readonly MoyasarConfigValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public void Fails_validation_when_publishableKey_is_missing_or_empty(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.PublishableKey = value;

        // Act
        var result = _validator.Validate(null, config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("MoyasarPaymentProvider:PublishableKey must not be empty.", result.FailureMessage);
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
        Assert.Equal("MoyasarPaymentProvider:Key must not be empty.", result.FailureMessage);
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
        Assert.Equal("MoyasarPaymentProvider:PayoutAccountId must not be empty.", result.FailureMessage);
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

    private static MoyasarConfig GetValidConfig() => new()
    {
        PublishableKey = "test",
        Key = "test",
        PayoutAccountId = "test"
    };
}
