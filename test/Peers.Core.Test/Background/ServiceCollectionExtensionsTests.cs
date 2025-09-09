using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Peers.Core.Background;

namespace Peers.Core.Test.Background;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMessageBroker_registers_required_services()
    {
        // Arrange
        var consumerCount = 5;
        var services = new ServiceCollection().AddLogging();

        // Act
        services.AddMessageBroker(consumerCount);

        // Assert
        var sp = services.BuildServiceProvider();
        sp.GetRequiredService<IProducer>();
        Assert.Equal(consumerCount, sp.GetRequiredService<IEnumerable<IConsumer>>().Count());
        Assert.IsType<MessageBroker>(sp.GetRequiredService<IHostedService>());
    }
}
