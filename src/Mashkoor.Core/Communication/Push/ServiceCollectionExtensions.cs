using Mashkoor.Core.Common.Configs;
using Mashkoor.Core.Communication.Push.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mashkoor.Core.Communication.Push;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds push notification services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddPushNotifications(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddSingleton<IFirebaseMessagingWrapper, FirebaseMessagingWrapper>()
            .AddSingleton<IFirebaseMessagingService, FirebaseMessagingService>()
            .AddScoped<IPushNotificationService, PushNotificationService>()
            .RegisterConfig<FirebaseConfig, FirebaseConfigValidator>(config)
            .TryAddSingleton(TimeProvider.System);

        return services;
    }
}
