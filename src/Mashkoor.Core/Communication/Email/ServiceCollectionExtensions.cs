using MailKit.Net.Smtp;
using Mashkoor.Core.Common.Configs;
using Mashkoor.Core.Communication.Email.Configuration;

namespace Mashkoor.Core.Communication.Email;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds email service configuration which are stored in configuration files.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddEmail(
        this IServiceCollection services,
        IConfiguration config)
        => services
            .RegisterConfig<EmailConfig, EmailConfigValidator>(config)
            .AddTransient<ISmtpClient, SmtpClient>()
            .AddScoped<IEmailService, EmailService>();
}
