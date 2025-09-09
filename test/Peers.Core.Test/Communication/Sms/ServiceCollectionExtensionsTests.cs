using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Peers.Core.Communication.Sms;
using Peers.Core.Communication.Sms.Configuration;

namespace Peers.Core.Test.Communication.Sms;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSms_adds_required_services()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "sms:sender", "Peers" },
                { "sms:key", "123" },
                { "sms:enabled", "false" },
            })
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddSms(config)
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<ISmsService>();
        serviceProvider.GetRequiredService<IValidateOptions<SmsConfig>>();
        serviceProvider.GetRequiredService<SmsConfig>();
        Assert.IsType<TaqnyatSmsServiceProvider>(serviceProvider.GetRequiredService<ISmsServiceProvider>());
    }
}
