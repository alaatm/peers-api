using Mashkoor.Core.AzureServices.AppInsights;
using Mashkoor.Core.AzureServices.Configuration;
using Mashkoor.Core.AzureServices.Storage;
using Mashkoor.Core.Common.Configs;
using Mashkoor.Core.Identity;
using Microsoft.ApplicationInsights.Extensibility;

namespace Mashkoor.Core.AzureServices;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds required azure storage services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddAzureStorage(this IServiceCollection services, IConfiguration config)
        => services
            .RegisterConfig<AzureConfig, AzureConfigValidator>(config)
            .AddSingleton<IStorageManager, StorageManager>();

    /// <summary>
    /// Adds Azure application insights services, including authenticated user enrichment.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns></returns>
    public static IServiceCollection AddAzureAppInsights(this IServiceCollection services)
        => services
            .AddIdentityInfo()
            .AddApplicationInsightsTelemetry()
            // This can be enabled in test environments to track sql commands
            //.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, _) => module.EnableSqlCommandTextInstrumentation = true)
            .AddSingleton<ITelemetryInitializer, TelemetryEnrichment>();
}
