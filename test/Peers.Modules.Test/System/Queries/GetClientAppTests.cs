using Peers.Modules.System.Domain;
using Peers.Modules.System.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Peers.Modules.Test.System.Queries;

[Collection(nameof(IntegrationTestBaseCollection))]
public class GetClientAppTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_staff_role()
        => await AssertCommandAccess(new GetClientApp.Query(), [Roles.Staff]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_clientApp()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        ExecuteDbContext(db =>
        {
            db.ClientApps.Add(new ClientAppInfo
            {
                PackageName = "com.peersapp.peers",
                HashString = "hash",
                AndroidStoreLink = "a",
                IOSStoreLink = "b",
                LatestVersion = new ClientAppVersion
                {
                    Major = 0,
                    Minor = 0,
                    Build = 0,
                    Revision = 0
                },
            });
            db.SaveChanges();
        });

        // Act
        var result = await SendAsync(new GetClientApp.Query(), manager);

        // Assert
        var okResult = Assert.IsType<Ok<GetClientApp.Response>>(result);
        var app = Assert.IsType<GetClientApp.Response>(okResult.Value);

        Assert.Equal("com.peersapp.peers", app.PackageName);
        Assert.Equal("hash", app.HashString);
        Assert.Equal("a", app.AndroidStoreLink);
        Assert.Equal("b", app.IosStoreLink);
        Assert.Equal("0.0.0 (0)", app.VersionString);
    }
}
