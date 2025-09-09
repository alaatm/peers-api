using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.RateLimiting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        [NotNull] IConfiguration config)
        => services.AddRateLimiter(new GenericRateLimiter(config).Configure);
}
