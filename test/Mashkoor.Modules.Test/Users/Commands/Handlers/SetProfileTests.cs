using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Queries;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class SetProfileTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_role()
        => await AssertCommandAccess(TestQuery(), [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_conflict_when_email_is_already_taken()
    {
        // Arrange
        var email = "email@example.com";
        var manager = await InsertManagerAsync(email: email);
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(TestQuery(email: email), customer);

        // Assert
        var conflictResult = AssertX.IsType<Conflict<ProblemDetails>>(result);
        var problem = conflictResult.Value;
        Assert.Equal("Email address already taken.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Sets_profile_and_returns_set_data()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var phone = customer.Username;

        // Act
        var result = await SendAsync(TestQuery("John", "Doe", "email@example.com", "ar"), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<GetProfile.Response>>(result);
        var response = okResult.Value;
        customer = await FindAsync<Customer>(p => p.Username == customer.Username, "User");

        Assert.Equal("John", customer.User.Firstname);
        Assert.Equal("Doe", customer.User.Lastname);
        Assert.Equal("email@example.com", customer.User.UpdatedEmail);
        Assert.Equal("ar", customer.User.PreferredLanguage);

        Assert.Equal("John", response.Firstname);
        Assert.Equal("Doe", response.Lastname);
        Assert.Equal(phone, response.Phone);
        Assert.Equal("email@example.com", response.Email);
        Assert.False(response.IsVerifiedEmail);
        Assert.Equal("ar", response.PreferredLanguage);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Resets_emailConfirmed_flag_when_email_is_changed()
    {
        // Arrange
        var customer = await EnrollCustomer();
        await ExecuteDbContextAsync(async db =>
        {
            var user = await db.Users.FindAsync(customer.Id);
            user.Email = "current@example.com";
            user.EmailConfirmed = true;
            await db.SaveChangesAsync();
        });

        // Act
        var result = await SendAsync(TestQuery(email: "new@example.com"), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<GetProfile.Response>>(result);
        var response = okResult.Value;
        customer = await FindAsync<Customer>(p => p.Username == customer.Username, "User");

        Assert.Equal("current@example.com", customer.User.Email);
        Assert.Equal("new@example.com", customer.User.UpdatedEmail);
        Assert.False(customer.User.EmailConfirmed);

        Assert.Equal("new@example.com", response.Email);
        Assert.False(response.IsVerifiedEmail);
    }

    private static SetProfile.Command TestQuery(string firstname = null, string lastname = null, string email = null, string preferredLang = null)
        => new(firstname, lastname, email, preferredLang);
}
