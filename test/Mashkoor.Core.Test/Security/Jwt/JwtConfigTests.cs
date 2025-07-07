using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Mashkoor.Core.Security.Jwt;

namespace Mashkoor.Core.Test.Security.Jwt;

public class JwtConfigTests
{
    private readonly JwtConfigValidator _validator = new();

    [Fact]
    public void KeyBytes_returns_base64_decoded_key()
    {
        // Arrange
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);

        var config = GetValidConfig();
        config.Key = Convert.ToBase64String(key);

        // Act
        var keyBytes = config.KeyBytes;

        // Assert
        Assert.Equal(key, keyBytes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid uri")]
    public void Validation_fails_when_invalid_issuer_setting(string issuer)
    {
        // Arrange
        var config = GetValidConfig();
        config.Issuer = issuer;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("jwt:Issuer must be a valid URI.", result.FailureMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid base64")]
    public void Validation_fails_when_invalid_key_setting(string key)
    {
        // Arrange
        var config = GetValidConfig();
        config.Key = key;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("jwt:Key must be a base64 encoded string.", result.FailureMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void Validation_fails_when_invalid_durationInMinutes_setting(int durationInMinutes)
    {
        // Arrange
        var config = GetValidConfig();
        config.DurationInMinutes = durationInMinutes;

        // Act
        var result = _validator.Validate("", config);

        // Assert
        Assert.True(result.Failed);
        Assert.Equal("jwt:DurationInMinutes must be set to 10 or greater.", result.FailureMessage);
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
    public void TokenValidationParameters_returns_valid_params()
    {
        // Arrange
        var config = GetValidConfig();

        // Act
        var tvp = config.TokenValidationParameters;

        // Assert
        Assert.True(tvp.ValidateIssuer);
        Assert.False(tvp.ValidateAudience);
        Assert.True(tvp.ValidateLifetime);
        Assert.True(tvp.ValidateIssuerSigningKey);
        Assert.Equal(config.Issuer, tvp.ValidIssuer);
        Assert.Null(tvp.ValidAudience);
        Assert.Null(tvp.AudienceValidator);
        Assert.NotNull(tvp.IssuerSigningKey);
        Assert.Equal(config.KeyBytes, ((SymmetricSecurityKey)tvp.IssuerSigningKey).Key);
    }

    private static JwtConfig GetValidConfig() => new()
    {
        Issuer = "https://www.jwt-test.com/iss",
        Key = "MDEyMzQ1Njc4OTAxMjM0NTY=",
        DurationInMinutes = 10,
    };
}
