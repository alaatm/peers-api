using Peers.Core.Security.Totp.Configuration;

namespace Peers.Core.Test.Security.Totp.Configuration;

public class TotpConfigTests
{
    private readonly TotpConfigValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("00000")]
    public void Validation_fails_when_UseDefaultOtp_is_true_and_DefaultOtp_is_not_a_4_digit_code(string defaultOtp)
    {
        // Arrange
        var config = GetValidConfig();
        config.UseDefaultOtp = true;
        config.DefaultOtp = defaultOtp;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("totp:DefaultOtp must be a 4 digit code.", result.FailureMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void Validation_fails_when_invalid_duration_setting(int durationInMinutes)
    {
        // Arrange
        var config = GetValidConfig();
        config.Duration = TimeSpan.FromMinutes(durationInMinutes);

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("totp:Duration must be set to 3 minutes or greater.", result.FailureMessage);
    }

    [Fact]
    public void Validation_passes_when_all_settings_are_valid()
    {
        // Act
        var result = _validator.Validate("", GetValidConfig());

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validation_passes_when_all_settings_are_valid_with_defaultOtp()
    {
        // Act
        var result = _validator.Validate("", GetValidConfigDefaultOtp());

        // Assert
        Assert.True(result.Succeeded);
    }

    private static TotpConfig GetValidConfig() => new()
    {
        UseDefaultOtp = false,
        Duration = TimeSpan.FromMinutes(3),
    };

    private static TotpConfig GetValidConfigDefaultOtp() => new()
    {
        UseDefaultOtp = true,
        DefaultOtp = "0000",
        Duration = TimeSpan.FromMinutes(3),
    };
}
