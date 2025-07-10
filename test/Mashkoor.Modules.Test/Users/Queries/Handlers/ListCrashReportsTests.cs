using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Queries;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Queries;

namespace Mashkoor.Modules.Test.Users.Queries.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ListCrashReportsTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_usersManager_role()
        => await AssertCommandAccess(TestQuery, [Roles.UsersManager]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_crash_reports_list()
    {
        // Arrange
        var cmd = new ReportDeviceError.Command(Guid.NewGuid(), "username", "en",
            true, "Platform", "1.0.0", "resumed", "exception", "0\n1", "1\n2", "deviceInfo", ReportDeviceError.Handler.Key);
        var id = AssertX.IsType<Created<IdObj>>(await SendAsync(cmd)).ExtractId();

        // Act
        var result = await SendAsync(TestQuery, await InsertManagerAsync());

        // Assert
        var okResult = Assert.IsType<Ok<PagedQueryResponse<DeviceError>>>(result);
        var response = okResult.Value;
        Assert.Equal(1, response.Total);
        var crashReport = Assert.Single(response.Data);
        Assert.Equal(cmd.DeviceId, crashReport.DeviceId);
        Assert.Equal(cmd.Username, crashReport.Username);
        Assert.Equal(cmd.Locale, crashReport.Locale);
        Assert.True(crashReport.Silent);
        Assert.Equal(cmd.Source, crashReport.Source);
        Assert.Equal(cmd.AppVersion, crashReport.AppVersion);
        Assert.Equal(cmd.AppState, crashReport.AppState);
        Assert.Equal(cmd.Exception, crashReport.Exception);
        Assert.Equal(cmd.StackTrace.Split('\n'), crashReport.StackTrace);
        Assert.Equal(cmd.Info.Split('\n'), crashReport.Info);
        Assert.Equal(cmd.DeviceId, crashReport.DeviceId);
    }

    private static ListCrashReports.Query TestQuery => new(null, null, null, null, null);
}
