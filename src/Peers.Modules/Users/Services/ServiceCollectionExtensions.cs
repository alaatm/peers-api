using Peers.Core.Communication.Push;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Peers.Modules.Users.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers required services for reporting push notification problems.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddPushNotificationProblemReporter(
        this IServiceCollection services)
    {
        services
            .AddScoped<IPushNotificationProblemReporter, PushNotificationProblemReporter>()
            .TryAddSingleton(TimeProvider.System);

        return services;
    }
}
