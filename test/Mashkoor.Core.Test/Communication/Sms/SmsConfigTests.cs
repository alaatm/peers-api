using Mashkoor.Core.Communication.Sms.Configuration;

namespace Mashkoor.Core.Test.Communication.Sms;

public class SmsConfigTests
{
    private readonly SmsConfigValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public void Fails_validation_when_sender_is_missing_or_empty(string sender)
    {
        // Arrange
        var config = GetValidConfig();
        config.Sender = sender;

        // Act
        var result = _validator.Validate(null, config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Sms:Sender must not be empty.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public void Fails_validation_when_key_is_missing_or_empty(string key)
    {
        // Arrange
        var config = GetValidConfig();
        config.Key = key;

        // Act
        var result = _validator.Validate(null, config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Sms:Key must not be empty.", result.FailureMessage);
    }

    [Fact]
    public void Succeeds_validation_when_sender_and_key_are_present()
    {
        // Act
        var result = _validator.Validate("", GetValidConfig());

        // Assert
        Assert.True(result.Succeeded);
    }

    private static SmsConfig GetValidConfig() => new()
    {
        Sender = "test",
        Key = "test",
    };
}
