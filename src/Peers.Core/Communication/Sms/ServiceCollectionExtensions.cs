using Peers.Core.Common.HttpClients;
using Peers.Core.Communication.Sms.Configuration;

namespace Peers.Core.Communication.Sms;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SMS services and configuration which are stored in configuration files.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddSms(this IServiceCollection services, IConfiguration config) => services
        .RegisterHttpClient<ISmsServiceProvider, TaqnyatSmsServiceProvider, SmsConfig, SmsConfigValidator>(config)
        .AddScoped<ISmsService, SmsService>();
}
