#if DEBUG
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
#endif

namespace Peers.Core.Localization;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalizationWithTracking(this IServiceCollection services)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddOptions();

        services.TryAddSingleton<IMissingKeyTrackerService, MissingKeyTrackerService>();
        services.TryAddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(TrackingStringLocalizer<>));
        services.AddHttpContextAccessor();

        return services;
#else
        services.AddLocalization();
        return services;
#endif
    }
}
