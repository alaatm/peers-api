using Peers.Core.Communication.Email.Configuration;

namespace Peers.Core.Test.Communication.Email;

public class EmailConfigTests
{
    private readonly EmailConfigValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid uri")]
    public void Validation_fails_when_invalid_host_setting(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.Host = value;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Email:Host must be a valid SMTP host URI.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validation_fails_when_invalid_username(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.Username = value;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Email:Username must not be empty.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validation_fails_when_invalid_password(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.Password = value;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Email:Password must not be empty.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validation_fails_when_invalid_senderName(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.SenderName = value;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Email:SenderName must not be empty.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid email")]
    public void Validation_fails_when_invalid_senderEmail(string value)
    {
        // Arrange
        var config = GetValidConfig();
        config.SenderEmail = value;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Email:SenderEmail must be a valid email address.", result.FailureMessage);
    }

    [Fact]
    public void Validation_fails_when_invalid_port()
    {
        // Arrange
        var config = GetValidConfig();
        config.Port = 0;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("Email:Port must not be empty.", result.FailureMessage);
    }

    [Fact]
    public void Validation_succeeds_when_valid_settings_supplied()
    {
        // Act
        var result = _validator.Validate("", GetValidConfig());

        // Assert
        Assert.True(result.Succeeded);
    }

    private static EmailConfig GetValidConfig() => new()
    {
        Host = "smtp.peers.com",
        Username = "Peers",
        Password = "123",
        SenderName = "Peers",
        SenderEmail = "email@peers.com",
        Port = 995,
        EnableSsl = true,
    };
}
