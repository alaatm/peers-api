using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Rules;

namespace Mashkoor.Modules.Test.User.Rules;

public class CanChangeUserStatusRuleTests : DomainEntityTestBase
{
    [Fact]
    public void Sets_correct_title()
    {
        // Arrange and act
        var errorTitle = new CanChangeUserStatusRule(default, default).ErrorTitle;

        // Assert
        Assert.Equal("Error changing user status", errorTitle);
    }

    [Theory]
    [InlineData(UserStatus.None)]
    [InlineData(UserStatus.Active)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Banned)]
    public void Reports_error_when_account_is_deleted_and_status_changing_to_a_value_other_than_deleted(UserStatus newStatus)
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.DeleteAccount(DateTime.UtcNow);
        var rule = new CanChangeUserStatusRule(user, newStatus);

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.True(result);
        Assert.Equal("Account is deleted.", Assert.Single(rule.Errors));
    }

    [Fact]
    public void Reports_error_when_changing_to_UserStatusNone()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var rule = new CanChangeUserStatusRule(user, UserStatus.None);

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.True(result);
        Assert.Equal("The status 'None' cannot be used.", Assert.Single(rule.Errors));
    }

    [Fact]
    public void Reports_error_when_new_and_old_status_are_the_same()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var rule = new CanChangeUserStatusRule(user, UserStatus.Active);

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.True(result);
        Assert.Equal($"The current user status is already set to 'Active'.", Assert.Single(rule.Errors));
    }

    [Fact]
    public void Passes_when_all_rules_are_adhered_to_for_driver()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var rule = new CanChangeUserStatusRule(user, UserStatus.Suspended);

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Passes_when_account_is_deleted_and_changing_status_to_deleted()
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.DeleteAccount(DateTime.UtcNow);
        var rule = new CanChangeUserStatusRule(user, UserStatus.Deleted);

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.False(result);
    }
}
