using Mashkoor.Core.AzureServices.Configuration;

namespace Mashkoor.Core.Test.AzureServices.Configuration;

public class AzureConfigTests
{
    private readonly AzureConfigValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validation_fails_when_invalid_storage_connection_string(string storageConnectionString)
    {
        // Arrange
        var config = new AzureConfig
        {
            StorageConnectionString = storageConnectionString,
        };

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("azure:StorageConnectionString must not be empty.", result.FailureMessage);
    }

    [Fact]
    public void Validation_succeeds_when_valid_settings_supplied()
    {
        // Act
        var config = new AzureConfig
        {
            StorageConnectionString = "X=Z",
        };
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Succeeded);
    }
}
