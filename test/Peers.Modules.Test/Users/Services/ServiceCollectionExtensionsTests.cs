using Microsoft.Extensions.DependencyInjection;
using Peers.Core.Communication.Push;
using Peers.Modules.Users.Services;

namespace Peers.Modules.Test.Users.Services;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPushNotificationProblemReporter_adds_all_required_services()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddDbContext<PeersContext>()
            .AddPushNotificationProblemReporter()
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IPushNotificationProblemReporter>();
    }
}
