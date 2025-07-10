using Microsoft.Extensions.DependencyInjection;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Modules.Users.Services;

namespace Mashkoor.Modules.Test.Users.Services;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPushNotificationProblemReporter_adds_all_required_services()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddSingleton(TimeProvider.System)
            .AddDbContext<MashkoorContext>()
            .AddPushNotificationProblemReporter()
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IPushNotificationProblemReporter>();
    }
}
