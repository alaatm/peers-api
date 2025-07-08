using Mashkoor.Core.Common.Configs;
using Mashkoor.Core.Communication.Push.Configuration;

namespace Mashkoor.Core.Communication.Push;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds push notification services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns></returns>
    public static IServiceCollection AddPushNotifications(
        this IServiceCollection services,
        IConfiguration config)
        => services
            .AddScoped<IFirebaseMessagingWrapper, FirebaseMessagingWrapper>()
            .AddScoped<IFirebaseMessagingService, FirebaseMessagingService>()
            .AddScoped<IPushNotificationService, PushNotificationService>()
            .RegisterConfig<FirebaseConfig, FirebaseConfigValidator>(config);
}
