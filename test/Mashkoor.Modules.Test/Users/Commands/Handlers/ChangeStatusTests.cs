using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ChangeStatusTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_usersManager_role()
        => await AssertCommandAccess(new ChangeStatus.Command(default, default, default), [Roles.UsersManager]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_user_does_not_exist()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(new ChangeStatus.Command(9999, UserStatus.Suspended, "test"), manager);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("User not found.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Can_suspend_user()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new ChangeStatus.Command(customer.User.Id, UserStatus.Suspended, "test"), manager);

        // Assert
        Assert.IsType<NoContent>(result);
        customer = await FindAsync<Customer>(customer.Id, "User");
        Assert.Equal(UserStatus.Suspended, customer.User.Status);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Can_ban_user()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new ChangeStatus.Command(customer.User.Id, UserStatus.Banned, "test"), manager);

        // Assert
        Assert.IsType<NoContent>(result);
        customer = await FindAsync<Customer>(customer.Id, "User");
        Assert.Equal(UserStatus.Banned, customer.User.Status);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Can_activate_user()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();
        Assert.IsType<NoContent>(await SendAsync(new ChangeStatus.Command(customer.User.Id, UserStatus.Banned, "test"), manager));

        // Act
        var result = await SendAsync(new ChangeStatus.Command(customer.User.Id, UserStatus.Active, "test"), manager);

        // Assert
        Assert.IsType<NoContent>(result);
        customer = await FindAsync<Customer>(customer.Id, "User");
        Assert.Equal(UserStatus.Active, customer.User.Status);
    }
}
