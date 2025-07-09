using Microsoft.ApplicationInsights.Extensibility;
using Mashkoor.Core.AzureServices.AppInsights;
using Mashkoor.Core.Identity;

namespace Mashkoor.Core.AzureServices;

public static partial class ServiceCollectionExtensions
{
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
