using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mashkoor.Core.Background.Jobs;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the specified job to be run in the background as specified in the cron expression of the job.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <typeparam name="T">The type of the job to add.</typeparam>
    /// <returns></returns>
    public static IServiceCollection AddBackgroundJob<T>(this IServiceCollection services)
        where T : IJob
    {
        services
            .AddHostedService<BackgroundJob<T>>()
            .TryAddSingleton(TimeProvider.System);

        return services;
    }
}
