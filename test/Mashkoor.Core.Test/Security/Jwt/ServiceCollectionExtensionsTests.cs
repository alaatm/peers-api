using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mashkoor.Core.Security.Jwt;

namespace Mashkoor.Core.Test.Security.Jwt;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddJwt_registers_required_auth_services()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "jwt:issuer", "https://www.jwt-test.com/iss" },
                { "jwt:key", Convert.ToBase64String(new byte[32]) },
                { "jwt:durationInMinutes", "10" },
            })
            .Build();

        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddSingleton<IConfiguration>(config)
            .AddJwt(config)
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IValidateOptions<JwtConfig>>();
        var jwtConfig = serviceProvider.GetRequiredService<JwtConfig>();
        serviceProvider.GetRequiredService<IJwtProvider>();
        serviceProvider.GetRequiredService<IAuthenticationService>();
        serviceProvider.GetRequiredService<IClaimsTransformation>();
        serviceProvider.GetRequiredService<IAuthenticationHandlerProvider>();
        serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        serviceProvider.GetRequiredService<IDataProtectionProvider>();
        serviceProvider.GetRequiredService<IOptions<JwtBearerOptions>>();
        Assert.Equal(
            JwtBearerDefaults.AuthenticationScheme,
            serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value.DefaultScheme);

        // Assert correct delegate is set
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = await schemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        var options = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);
        // Assert correct token validation parameters are set
        var expected = jwtConfig.TokenValidationParameters;
        var actual = options.TokenValidationParameters;
        Assert.Equal(expected.NameClaimType, actual.NameClaimType);
        Assert.Equal(expected.RoleClaimType, actual.RoleClaimType);
        Assert.Equal(expected.ValidIssuer, actual.ValidIssuer);
        Assert.Equal(expected.ValidateIssuer, actual.ValidateIssuer);
        Assert.Equal(expected.ValidAudience, actual.ValidAudience);
        Assert.Equal(expected.ValidateAudience, actual.ValidateAudience);
        Assert.Equal(expected.AudienceValidator, actual.AudienceValidator);
        Assert.Equal(expected.ValidateLifetime, actual.ValidateLifetime);
        Assert.Equal(expected.ValidateIssuerSigningKey, actual.ValidateIssuerSigningKey);
        Assert.Equal(((SymmetricSecurityKey)expected.IssuerSigningKey).Key, ((SymmetricSecurityKey)actual.IssuerSigningKey).Key);
    }
}
