using Mashkoor.Core.Common.Configs;
using Mashkoor.Core.Security.Totp.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mashkoor.Core.Security.Totp;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for TOTP token generation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddTotpTokenProvider(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddMemoryCache()
            .RegisterConfig<TotpConfig, TotpConfigValidator>(config)
            .AddSingleton<ITotpTokenProvider, TotpTokenProvider>()
            .TryAddSingleton(TimeProvider.System);

        return services;
    }
}

