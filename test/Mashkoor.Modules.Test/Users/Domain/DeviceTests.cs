namespace Mashkoor.Modules.Test.Users.Domain;

public class DeviceTests
{
    [Fact]
    public void IsStalled_returns_true_when_timestamp_is_more_than_61_days_old()
    {
        // Arrange
        var device = TestDevice(null).Generate();
        device.PnsHandleLastRefreshed = DateTime.UtcNow.AddDays(-62);

        // Act
        var isStalled = device.IsStalled(DateTime.UtcNow);

        // Assert
        Assert.True(isStalled);
    }

    [Fact]
    public void IsStalled_returns_false_when_timestamp_is_less_than_61_days_old()
    {
        // Arrange
        var device = TestDevice(null).Generate();
        device.PnsHandleLastRefreshed = DateTime.UtcNow.AddDays(-60);

        // Act
        var isStalled = device.IsStalled(DateTime.UtcNow);

        // Assert
        Assert.False(isStalled);
    }

    [Fact]
    public void UpdateHandle_updates_handle_and_timestamp()
    {
        // Arrange
        var date1 = DateTime.UtcNow.AddYears(-1);
        var date2 = DateTime.UtcNow;

        var handle = Guid.NewGuid().ToString();
        var device = TestDevice(null).Generate();
        device.UpdateHandle(date1, Guid.NewGuid().ToString());
        Assert.Equal(date1, device.PnsHandleTimestamp);
        Assert.Equal(date1, device.PnsHandleLastRefreshed);

        // Act
        device.UpdateHandle(date2, handle);

        // Assert
        Assert.Equal(handle, device.PnsHandle);
        Assert.Equal(date2, device.PnsHandleTimestamp);
        Assert.Equal(date2, device.PnsHandleLastRefreshed);
    }

    [Fact]
    public void UpdateHandle_updates_handle_and_timestamp_only_when_changed()
    {
        // Arrange
        var date1 = DateTime.UtcNow.AddYears(-1);
        var date2 = DateTime.UtcNow;

        var handle = Guid.NewGuid().ToString();
        var device = TestDevice(null).Generate();
        device.UpdateHandle(date1, handle);
        Assert.Equal(date1, device.PnsHandleTimestamp);
        Assert.Equal(date1, device.PnsHandleLastRefreshed);

        // Act
        device.UpdateHandle(date2, handle);

        // Assert
        Assert.Equal(handle, device.PnsHandle);
        Assert.Equal(date1, device.PnsHandleTimestamp);
        // Last refreshed is always updated.
        Assert.Equal(date2, device.PnsHandleLastRefreshed);
    }

    [Fact]
    public void UpdateHandle_can_remove_handle()
    {
        // Arrange
        var device = TestDevice(null).Generate();
        device.UpdateHandle(DateTime.UtcNow, Guid.NewGuid().ToString());

        // Act
        device.UpdateHandle(DateTime.UtcNow, null);

        // Assert
        Assert.Null(device.PnsHandle);
    }

    [Fact]
    public void UpdateAppVersion_updates_app_version()
    {
        // Arrange
        var version1 = "1.0.0";
        var version2 = "2.0.0";
        var device = TestDevice(null).Generate();
        device.UpdateAppVersion(version1);

        // Act
        device.UpdateAppVersion(version2);

        // Assert
        Assert.Equal(version2, device.AppVersion);
    }

    [Fact]
    public void UpdateAppVersion_noops_when_passing_same_version()
    {
        // Arrange
        var version = "1.0.0";
        var device = TestDevice(null).Generate();
        device.UpdateAppVersion(version);

        // Act
        device.UpdateAppVersion(version);

        // Assert
        Assert.Equal(version, device.AppVersion);
    }
}
