using Peers.Core.GoogleServices.Configuration;

namespace Peers.Core.Test.GoogleServices;

public class GoogleConfigTests
{
    private readonly GoogleConfigValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public void Validation_fails_when_invalid_api_key(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.ApiKey = value;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Google:ApiKey must not be empty.", result.FailureMessage);
    }

    [Fact]
    public void Validation_succeeds_when_valid_config()
    {
        // Arrange
        var config = GetValidConfig();
        // Act
        var result = _validator.Validate("", config);
        // Assert
        Assert.False(result.Failed);
    }

    private static GoogleConfig GetValidConfig() => new()
    {
        ApiKey = "123"
    };
}
