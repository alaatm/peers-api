using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Mashkoor.Core.Queries;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Queries;

namespace Mashkoor.Modules.Test.Users.Queries.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ListAppUsageHistoryTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_usersManager_role()
        => await AssertCommandAccess(TestQuery, [Roles.UsersManager]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_app_usage_history()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        await ExecuteDbContextAsync(async db =>
        {
            customer = await db.Customers.Include(p => p.User.DeviceList).FirstAsync(p => p.Id == customer.Id);
            customer.User.RecordAppUsed(DateTime.UtcNow);
            await Task.Delay(1);
            customer.User.RecordAppUsed(DateTime.UtcNow);
            await Task.Delay(1);
            customer.User.RecordAppUsed(DateTime.UtcNow);
            await db.SaveChangesAsync();
        });

        // Act
        var result = await SendAsync(TestQuery with { Id = customer.Id }, manager);

        // Assert
        var okResult = Assert.IsType<Ok<PagedQueryResponse<AppUsageHistory>>>(result);
        var response = okResult.Value;
        Assert.Equal(3, response.Total);

        Assert.True(response.Data[0].OpenedAt > response.Data[1].OpenedAt);
        Assert.True(response.Data[1].OpenedAt > response.Data[2].OpenedAt);
    }

    private static ListAppUsageHistory.Query TestQuery => new(1, null, null, null, null, null);
}
