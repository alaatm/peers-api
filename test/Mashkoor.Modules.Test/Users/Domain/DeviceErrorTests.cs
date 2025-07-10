using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Test.Users.Domain;

public class DeviceErrorTests
{
    [Fact]
    public void Create_creates_instance()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var deviceId = Guid.NewGuid();
        var username = "username";
        var locale = "en";
        var silent = true;
        var source = "Flutter";
        var appVersion = "1.0.0";
        var appState = "detached";
        var exception = "exception";
        var stackTrace = new[] { "0", "1" };
        var info = new[] { "0", "1" };
        var deviceInfo = "deviceInfo";

        // Act
        var deviceError = DeviceError.Create(
            date, deviceId, username, locale, silent, source,
            appVersion, appState, exception, stackTrace, info, deviceInfo);

        // Assert
        Assert.Equal(date, deviceError.ReportedOn);
        Assert.Equal(deviceId, deviceError.DeviceId);
        Assert.Equal(username, deviceError.Username);
        Assert.Equal(locale, deviceError.Locale);
        Assert.Equal(silent, deviceError.Silent);
        Assert.Equal(source, deviceError.Source);
        Assert.Equal(appVersion, deviceError.AppVersion);
        Assert.Equal(appState, deviceError.AppState);
        Assert.Equal(exception, deviceError.Exception);
        Assert.Equal(stackTrace, deviceError.StackTrace);
        Assert.Equal(info, deviceError.Info);
        Assert.Equal(deviceInfo, deviceError.DeviceInfo);
    }
}
