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
        Assert.IsType<GetSystemInfo.Response>(okResult.Value);
    }
}
