using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mashkoor.Core.Security.Totp;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for TOTP token generation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns></returns>
    public static IServiceCollection AddTotpTokenProvider(this IServiceCollection services)
    {
        services
            .AddMemoryCache()
            .AddSingleton<ITotpTokenProvider, TotpTokenProvider>()
            .TryAddSingleton(TimeProvider.System);

        return services;
    }
}

