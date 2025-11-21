using System.Diagnostics.CodeAnalysis;
using Peers.Core.Common.HttpClients;
using Peers.Core.GoogleServices.Configuration;
using Peers.Core.GoogleServices.Maps;

namespace Peers.Core.GoogleServices;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Google API services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddGoogleServices(
        [NotNull] this IServiceCollection services,
        [NotNull] IConfiguration config) => services
            .RegisterHttpClient<IGoogleMapsService, GoogleMapsService, GoogleConfig, GoogleConfigValidator>(config)
            .AddMemoryCache();
}
