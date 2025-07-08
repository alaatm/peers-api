using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mashkoor.Core.Communication.Email;
using Mashkoor.Core.Communication.Email.Configuration;

namespace Mashkoor.Core.Test.Communication.Email;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEmail_adds_required_services()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "email:host", "smtp.Mashkoor.com" },
                { "email:username", "username" },
                { "email:password", "password" },
                { "email:senderName", "Mashkoor" },
                { "email:senderEmail", "email@Mashkoor.com" },
                { "email:port", "995" },
                { "email:enableSsl", "true" },
                { "email:enabled", "false" },
            })
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddEmail(config)
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IEmailService>();
        serviceProvider.GetRequiredService<IValidateOptions<EmailConfig>>();
        serviceProvider.GetRequiredService<EmailConfig>();
    }
}
