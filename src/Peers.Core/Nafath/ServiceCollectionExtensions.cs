using Peers.Core.Common.HttpClients;
using Peers.Core.Nafath.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Nafath;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNafath(
        [NotNull] this IServiceCollection services,
        [NotNull] IConfiguration config) => services
            .RegisterHttpClient<INafathService, NafathService, NafathConfig, NafathConfigValidator>(config)
            .AddMemoryCache();
}
