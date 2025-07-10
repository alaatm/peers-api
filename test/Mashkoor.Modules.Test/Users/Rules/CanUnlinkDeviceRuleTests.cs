using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Rules;

namespace Mashkoor.Modules.Test.User.Rules;

public class CanUnlinkDeviceRuleTests : DomainEntityTestBase
{
    [Fact]
    public void Sets_correct_title()
    {
        // Arrange and act
        var errorTitle = new CanUnlinkDeviceRule(default, default).ErrorTitle;

        // Assert
        Assert.Equal("Error unlinking device", errorTitle);
    }

    [Fact]
    public void Reports_error_when_device_is_not_linked()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var rule = new CanUnlinkDeviceRule(user, TestDevice(null).Generate());

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.True(result);
        Assert.Equal("Cannot unlink an unlinked device.", Assert.Single(rule.Errors));
    }

    [Fact]
    public void Reports_error_when_device_is_linked_but_with_different_props()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var device = TestDevice(null).Generate();
        user.LinkDevice(device);
        device = new Device(DateTime.UtcNow, user, device.DeviceId, "different", device.Model, device.Platform, device.OSVersion, device.Idiom, device.DeviceType, device.PnsHandle, "x", "x");
        var rule = new CanUnlinkDeviceRule(user, device);

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.True(result);
        Assert.Equal("The device to be unlinked is found with matching id but the device props differ.", Assert.Single(rule.Errors));
    }

    [Fact]
    public void Passes_when_device_is_linked()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var rule = new CanUnlinkDeviceRule(user, user.DeviceList.Single());

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.False(result);
    }
}
