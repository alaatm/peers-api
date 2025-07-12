using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.System.Queries;

namespace Mashkoor.Modules.Test.System.Queries;

[Collection(nameof(IntegrationTestBaseCollection))]
public class GetSystemInfoTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_staff_role()
        => await AssertCommandAccess(new GetSystemInfo.Query(), [Roles.Staff]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_system_info()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(new GetSystemInfo.Query(), manager);

        // Assert
        var okResult = Assert.IsType<Ok<GetSystemInfo.Response>>(result);
        var data = Assert.IsType<GetSystemInfo.Response>(okResult.Value);

        Assert.NotNull(data.App);
        Assert.NotNull(data.Server);
        Assert.NotNull(data.Database);
        Assert.NotEmpty(data.App.Version);
        Assert.NotEmpty(data.Server.OSVersion);
        Assert.NotEmpty(data.Server.RuntimeVersion);
        Assert.NotEmpty(data.Database.Version);
        Assert.NotEmpty(data.Database.DbSize);
        Assert.NotEmpty(data.Database.Unallocated);
        Assert.NotEmpty(data.Database.Reserved);
        Assert.NotEmpty(data.Database.Data);
        Assert.NotEmpty(data.Database.IndexSize);
        Assert.NotEmpty(data.Database.Unused);
    }
}
