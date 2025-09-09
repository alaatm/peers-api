using Microsoft.AspNetCore.Http.HttpResults;
using Peers.Core.Commands;
using Peers.Core.Http;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ReportDeviceErrorTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_401Unauthorized_when_invalid_key()
    {
        // Arrange
        var cmd = new ReportDeviceError.Command(Guid.NewGuid(), null, null, default, null, null, null, null, null, null, null, "invalid");

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var unauthResult = Assert.IsType<UnauthorizedHttpResult2<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("Missing or invalid key.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Reports_device_error()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var username = "username";
        var locale = "en";
        var silent = true;
        var source = "flutter";
        var appVersion = "1.0.0";
        var appState = "inactive";
        var exception = "exception";
        var stackTrace = "0\n1";
        var info = "1\n2";
        var deviceInfo = "deviceInfo";
        var key = ReportDeviceError.Handler.Key;

        var cmd = new ReportDeviceError.Command(
            deviceId, username, locale, silent,
            source, appVersion, appState,
            exception, stackTrace, deviceInfo, info,
            key);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var id = Assert.IsType<Created<IdObj>>(result).ExtractId();
        var crashReport = await FindAsync<DeviceError>(id);
        Assert.Equal(cmd.DeviceId, crashReport.DeviceId);
        Assert.Equal(cmd.Username, crashReport.Username);
        Assert.Equal(cmd.Locale, crashReport.Locale);
        Assert.Equal(cmd.Silent, crashReport.Silent);
        Assert.Equal(cmd.Source, crashReport.Source);
        Assert.Equal(cmd.AppVersion, crashReport.AppVersion);
        Assert.Equal(cmd.AppState, crashReport.AppState);
        Assert.Equal(cmd.Exception, crashReport.Exception);
        Assert.Equal(cmd.StackTrace.Split('\n'), crashReport.StackTrace);
        Assert.Equal(cmd.Info.Split('\n'), crashReport.Info);
        Assert.Equal(cmd.DeviceId, crashReport.DeviceId);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Handles_empty_stackTrace()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var username = "username";
        var locale = "en";
        var silent = true;
        var source = "Platform";
        var appVersion = "1.0.0";
        var appState = "inactive";
        var exception = "exception";
        var stackTrace = "";
        var info = "";
        var deviceInfo = "deviceInfo";
        var key = ReportDeviceError.Handler.Key;

        var cmd = new ReportDeviceError.Command(
            deviceId, username, locale, silent,
            source, appVersion, appState,
            exception, stackTrace, info, deviceInfo,
            key);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var id = Assert.IsType<Created<IdObj>>(result).ExtractId();
        var crashReport = await FindAsync<DeviceError>(id);
        Assert.Equal(cmd.DeviceId, crashReport.DeviceId);
        Assert.Equal(cmd.Username, crashReport.Username);
        Assert.Equal(cmd.Locale, crashReport.Locale);
        Assert.Equal(cmd.Silent, crashReport.Silent);
        Assert.Equal(cmd.Source, crashReport.Source);
        Assert.Equal(cmd.AppVersion, crashReport.AppVersion);
        Assert.Equal(cmd.AppState, crashReport.AppState);
        Assert.Equal(cmd.Exception, crashReport.Exception);
        Assert.Equal([], crashReport.StackTrace);
        Assert.Equal([], crashReport.Info);
        Assert.Equal(cmd.DeviceId, crashReport.DeviceId);
    }
}
