using Mashkoor.Modules.Customers.Rules;

namespace Mashkoor.Modules.Test.Customers.Rules;

public class CanDeleteCustomerAccountRuleTests : DomainEntityTestBase
{
    [Fact]
    public void Sets_correct_title()
    {
        // Arrange and act
        var errorTitle = new CanDeleteCustomerAccountRule(default).ErrorTitle;

        // Assert
        Assert.Equal("Error deleting user account", errorTitle);
    }
}
