using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Time.Testing;
using Mashkoor.Core.Identity;
using Mashkoor.Core.Security.Jwt;

namespace Mashkoor.Core.Test.Security.Jwt;

public class JwtProviderTests
{
    private static readonly JwtConfig _config = new()
    {
        Issuer = "https://www.jwt-test.com/iss",
        Key = "jG8WgE/az5BqjRzOLZ1T8SkuVcbINk+CUF9GjxHeIks=",
        DurationInMinutes = 10,
    };

    [Fact]
    public void BuildToken_builds_correct_token()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(DateTime.UtcNow);
        var roles = new List<string>
        {
            "role1",
            "role2"
        };
        var claims = new List<Claim>
        {
            new("type1", "value1"),
            new("type2", "value2"),
        };

        var jwtProvider = new JwtProvider(timeProvider, _config);

        // Act
        var (token, expires) = jwtProvider.BuildToken(roles, claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);

        Assert.Equal(3 + roles.Count + claims.Count, jwtSecurityToken.Claims.Count());

        Assert.Equal(_config.Issuer, jwtSecurityToken.Issuer);
        Assert.Equal(timeProvider.UtcNow().AddMinutes(10), expires);

        // nbf not set
        Assert.Equal(default, jwtSecurityToken.ValidFrom);
        // IssuedAt rounds off to the nearest second
        Assert.True((timeProvider.UtcNow() - jwtSecurityToken.IssuedAt).TotalSeconds < 1);
        Assert.Equal(jwtSecurityToken.IssuedAt.AddMinutes(10), jwtSecurityToken.ValidTo);

        roles.ForEach(r => Assert.Single(jwtSecurityToken.Claims, p => p.Type == CustomClaimTypes.Role && p.Value == r));
        claims.ForEach(c => Assert.Single(jwtSecurityToken.Claims, p => p.Type == c.Type && p.Value == c.Value));

        // Assert validation doesn't throw
        handler.ValidateToken(token, _config.TokenValidationParameters, out _);
    }

    [Fact]
    public void BuildToken_does_not_perform_outbound_mapping()
    {
        // Arrange
        var jwtProvider = new JwtProvider(TimeProvider.System, _config);

        // Act
        var (token, expires) = jwtProvider.BuildToken([], [new Claim(ClaimTypes.Name, "test")]);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        // Assert that ClaimTypes.Name did not get mapped to JwtRegisteredClaimNames.UniqueName
        // as it would if outbound mapping was enabled (which is default)
        Assert.Contains(jwtSecurityToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "test");
    }

    [Fact]
    public void BuildToken_can_set_custom_expiry_date()
    {
        // Arrange
        var customExpiryDate = DateTime.UtcNow.AddYears(99);
        var timeProvider = new FakeTimeProvider(DateTime.UtcNow);

        // Act
        var (_, expires) = new JwtProvider(timeProvider, _config).BuildToken([], [], customExpiryDate);

        // Assert
        Assert.Equal(customExpiryDate, expires);
    }
}
