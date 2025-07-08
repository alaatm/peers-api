using Microsoft.Extensions.DependencyInjection;
using Mashkoor.Core.Security.Hashing;

namespace Mashkoor.Core.Test.Security.Hashing;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHmacHash_registers_required_services()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddHmacHash()
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IHmacHash>();
    }
}
