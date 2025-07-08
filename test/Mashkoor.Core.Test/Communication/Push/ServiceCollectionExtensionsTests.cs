using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Core.Communication.Push.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mashkoor.Core.Test.Communication.Push;

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
        serviceProvider.GetRequiredService<IFirebaseMessagingWrapper>();
        serviceProvider.GetRequiredService<IFirebaseMessagingService>();
        serviceProvider.GetRequiredService<IPushNotificationService>();
        serviceProvider.GetRequiredService<IValidateOptions<FirebaseConfig>>();
        serviceProvider.GetRequiredService<FirebaseConfig>();
    }

    private class PushNotificationProblemReporterMoq : IPushNotificationProblemReporter
    {
        public Task ReportErrorsAsync(ICollection<(string, ErrorCode, MessagingErrorCode?)> errors) => throw new NotImplementedException();
    }
}
