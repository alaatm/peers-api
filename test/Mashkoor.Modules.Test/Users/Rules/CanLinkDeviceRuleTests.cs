using Mashkoor.Modules.Users.Rules;

namespace Mashkoor.Modules.Test.User.Rules;

public class CanLinkDeviceRuleTests : DomainEntityTestBase
{
    [Fact]
    public void Sets_correct_title()
    {
        // Arrange and act
        var errorTitle = new CanLinkDeviceRule(default, default).ErrorTitle;

        // Assert
        Assert.Equal("Error linking device", errorTitle);
    }

    [Fact]
    public void Reports_error_when_device_is_already_linked()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var rule = new CanLinkDeviceRule(user, user.DeviceList.Single());

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.True(result);
        Assert.Equal("The device is already linked.", Assert.Single(rule.Errors));
    }

    [Fact]
    public void Passes_when_device_is_not_linked()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var rule = new CanLinkDeviceRule(user, TestDevice(null).Generate());

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.False(result);
    }
}
