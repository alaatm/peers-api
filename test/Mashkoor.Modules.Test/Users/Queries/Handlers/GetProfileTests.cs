using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Queries;

namespace Mashkoor.Modules.Test.Users.Queries.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class GetProfileTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_or_driver_or_partner_or_customersManager_role()
        => await AssertCommandAccess(new GetProfile.Query(1), [Roles.Customer, Roles.UsersManager]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_executing_customer_sets_id()
    {
        // Arrange
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new GetProfile.Query(customer.Id), customer);

        // Assert
        var badResult = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badResult.Value);
        Assert.Equal("Use '/users/me/profile' for querying own profile.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_NotFound_when_user_not_found()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(new GetProfile.Query(9999), manager);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_user_profile()
    {
        // Arrange
        var customer = await EnrollCustomer();
        Assert.IsType<Ok<GetProfile.Response>>(await SendAsync(new SetProfile.Command("John", "Doe", null, "en"), customer));
        customer = await FindAsync<Customer>(customer.Id, "User");

        // Act
        var result = await SendAsync(new GetProfile.Query(null), customer);

        // Assert
        var okResult = Assert.IsType<Ok<GetProfile.Response>>(result);
        var response = okResult.Value;

        AssertUserProfile(customer.User, response);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_user_profile_executed_from_manager()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();
        Assert.IsType<Ok<GetProfile.Response>>(await SendAsync(new SetProfile.Command("John", "Doe", null, "en"), customer));
        customer = await FindAsync<Customer>(customer.Id, "User");

        // Act
        var result = await SendAsync(new GetProfile.Query(customer.Id), manager);

        // Assert
        var okResult = Assert.IsType<Ok<GetProfile.Response>>(result);
        var response = okResult.Value;

        AssertUserProfile(customer.User, response);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_updatedEmail_when_an_update_is_initiated_but_not_finished()
    {
        // Arrange
        var customer = await EnrollCustomer();
        Assert.IsType<Ok<GetProfile.Response>>(await SendAsync(new SetProfile.Command(null, null, "email@example.com", null), customer));
        customer = await FindAsync<Customer>(customer.Id, "User");

        // Act
        var result = await SendAsync(new GetProfile.Query(null), customer);

        // Assert
        var okResult = Assert.IsType<Ok<GetProfile.Response>>(result);
        var response = okResult.Value;

        Assert.Equal("email@example.com", response.Email);
    }

    private static void AssertUserProfile(
        AppUser user,
        GetProfile.Response response)
    {
        Assert.Equal(user.Firstname, response.Firstname);
        Assert.Equal(user.Lastname, response.Lastname);
        Assert.Equal(user.UserName, response.Phone);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(user.PreferredLanguage, response.PreferredLanguage);
    }
}
