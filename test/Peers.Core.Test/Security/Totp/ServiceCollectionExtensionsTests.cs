using Peers.Core.Security.Totp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Peers.Core.Test.Security.Totp;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTotpTokenProvider_Adds_totp_token_provider_as_singleton()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "totp:useDefaultOtp", "false" },
                { "totp:duration", "00:03:00" },
            })
            .Build();
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddTotpTokenProvider(config)
            .BuildServiceProvider();

        // Assert
        var scope1 = serviceProvider.CreateScope();
        var scope2 = serviceProvider.CreateScope();

        var instance1 = Assert.IsType<TotpTokenProvider>(scope1.ServiceProvider.GetRequiredService<ITotpTokenProvider>());
        var instance2 = Assert.IsType<TotpTokenProvider>(scope2.ServiceProvider.GetRequiredService<ITotpTokenProvider>());
        Assert.Same(instance1, instance2);
    }
}
