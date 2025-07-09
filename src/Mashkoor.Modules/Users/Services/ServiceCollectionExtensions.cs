using Mashkoor.Core.Communication.Push;

namespace Mashkoor.Modules.Users.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers required services for reporting push notification problems.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddPushNotificationProblemReporter(
        this IServiceCollection services)
        => services
            .AddScoped<IPushNotificationProblemReporter, PushNotificationProblemReporter>();
}
