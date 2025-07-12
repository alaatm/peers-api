using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class DeleteAccountTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_role()
        => await AssertCommandAccess(new DeleteAccount.Command(), [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Can_delete_customer_account()
    {
        // Arrange
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new DeleteAccount.Command(), customer);

        // Assert
        Assert.IsType<NoContent>(result);

        customer = await FindAsync<Customer>(customer.Id, "User");
        Assert.True(customer.User.IsDeleted);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Can_delete_user_account_after_recreate_with_same_username()
    {
        // Arrange
        var customer = await EnrollCustomer();
        Assert.IsType<NoContent>(await SendAsync(new DeleteAccount.Command(), customer));
        customer = await EnrollCustomer(username: customer.Username);

        // Act
        var result = await SendAsync(new DeleteAccount.Command(), customer);

        // Assert
        Assert.IsType<NoContent>(result);

        customer = await FindAsync<Customer>(customer.Id, "User");
        Assert.True(customer.User.IsDeleted);
    }
}
