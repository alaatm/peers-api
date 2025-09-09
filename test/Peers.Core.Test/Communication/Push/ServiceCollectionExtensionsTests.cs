using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Peers.Core.Communication.Push;
using Peers.Core.Communication.Push.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Peers.Core.Test.Communication.Push;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPushNotifications_adds_required_services()
    {
        // Arrange
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Firebase:ProjectId", "test" },
                { "Firebase:ServiceAccountKey", FirebaseMessagingServiceTests.TestFirebaseServiceAccount },
            })
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddScoped<IPushNotificationProblemReporter, PushNotificationProblemReporterMoq>()
            .AddPushNotifications(config)
            .BuildServiceProvider();

        // Assert
        Assert.Same(GetSingleton<IFirebaseMessagingWrapper>(), GetSingleton<IFirebaseMessagingWrapper>());
        Assert.Same(GetSingleton<IFirebaseMessagingService>(), GetSingleton<IFirebaseMessagingService>());
        serviceProvider.GetRequiredService<IPushNotificationService>();
        serviceProvider.GetRequiredService<IValidateOptions<FirebaseConfig>>();
        serviceProvider.GetRequiredService<FirebaseConfig>();

        T GetSingleton<T>()
            where T : class
        {
            using var scope = serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<T>();
        }
    }

    private class PushNotificationProblemReporterMoq : IPushNotificationProblemReporter
    {
        public Task ReportErrorsAsync(ICollection<(string, ErrorCode, MessagingErrorCode?)> errors) => throw new NotImplementedException();
    }
}
