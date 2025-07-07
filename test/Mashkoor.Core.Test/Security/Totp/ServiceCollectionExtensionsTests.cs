using Microsoft.Extensions.DependencyInjection;
using Mashkoor.Core.Security.Totp;

namespace Mashkoor.Core.Test.Security.Totp;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTotpTokenProvider_Adds_totp_token_provider_as_singleton()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddTotpTokenProvider()
            .BuildServiceProvider();

        // Assert
        var scope1 = serviceProvider.CreateScope();
        var scope2 = serviceProvider.CreateScope();

        var instance1 = Assert.IsType<TotpTokenProvider>(scope1.ServiceProvider.GetRequiredService<ITotpTokenProvider>());
        var instance2 = Assert.IsType<TotpTokenProvider>(scope2.ServiceProvider.GetRequiredService<ITotpTokenProvider>());
        Assert.Same(instance1, instance2);
    }
}
