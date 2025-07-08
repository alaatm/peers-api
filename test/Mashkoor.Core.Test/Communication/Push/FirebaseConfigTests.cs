using Mashkoor.Core.Communication.Push.Configuration;

namespace Mashkoor.Core.Test.Communication.Push;

public class FirebaseConfigTests
{
    private readonly FirebaseConfigValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validation_fails_when_invalid_projectId(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.ProjectId = value;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Firebase:ProjectId must not be empty.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid json")]
    public void Validation_fails_when_invalid_serviceAccountKey(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.ServiceAccountKey = value;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Firebase:ServiceAccountKey must be a valid JSON object.", result.FailureMessage);
    }

    [Fact]
    public void Validation_succeeds_when_valid_settings_supplied()
    {
        // Act
        var result = _validator.Validate("", GetValidConfig());

        // Assert
        Assert.True(result.Succeeded);
    }

    private static FirebaseConfig GetValidConfig() => new()
    {
        ProjectId = "test",
        ServiceAccountKey = "{}"
    };
}
