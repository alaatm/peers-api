using Peers.Modules.Users.Domain;
using Peers.Modules.Users.EventHandlers;
using Peers.Modules.Users.Events;

namespace Peers.Modules.Test.Users.EventHandlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class OnAppOpenedTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Inserts_app_usage_entry()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var customer = await EnrollCustomer();
        var notification = new AppOpened(date, IdentityMoq.Object, customer.Id);

        // Act
        await ExecuteDbContextAsync(async db =>
        {
            var handler = new OnAppOpened(db);
            await handler.Handle(notification, CancellationToken.None);
        });

        // Assert
        var user = await FindAsync<AppUser>(customer.Id, "AppUsage");
        var appUsage = Assert.Single(user.AppUsage);
        Assert.Equal(date, appUsage.OpenedAt);
    }
}
