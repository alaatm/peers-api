using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Mashkoor.Core.Common.Configs;

namespace Mashkoor.Core.Security.Jwt;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for JWT authentication in DI.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The root configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddJwt(
        this IServiceCollection services,
        [NotNull] IConfiguration config)
    {
        var jwtConfig = config.GetSection(JwtConfig.ConfigSection).Get<JwtConfig>() ?? throw new InvalidOperationException("JWT configuration is missing.");

        services
            .RegisterConfig<JwtConfig, JwtConfigValidator>(config)
            .AddSingleton<IJwtProvider, JwtProvider>()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(p =>
            {
                // Prevent inbound claims mapping so that we can use the specified claims types for name/role in the TokenValidationParameters
                p.MapInboundClaims = false;
                p.TokenValidationParameters = jwtConfig.TokenValidationParameters;
            });

        return services;
    }
}

