using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Peers.Core.Identity;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for identity in DI.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns></returns>
    public static IServiceCollection AddIdentityInfo(this IServiceCollection services)
    {
        services
            .AddHttpContextAccessor()
            // Must be scoped because we cache the user type in the identity info.
            .TryAddScoped<IIdentityInfo, IdentityInfo>();

        return services;
    }
}

