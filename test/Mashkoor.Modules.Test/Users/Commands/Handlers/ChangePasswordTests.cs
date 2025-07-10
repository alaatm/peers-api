using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Mashkoor.Core.Http;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ChangePasswordTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_auth()
        => await AssertCommandAccess(TestChangePassword.Generate());

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Forbidden_when_executed_from_a_non_password_account()
    {
        // Arrange
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(TestChangePassword.Generate(), customer);

        // Assert
        var unauthResult = Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("You are not authorized to perform this operation.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_password_change_fails()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(TestChangePassword.Generate() with { CurrentPassword = "00000000" }, manager);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Password change failed", problem.Detail);
        Assert.Equal("Incorrect password.", Assert.Single(problem.Extensions["errors"] as string[]));
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Changes_password()
    {
        // Arrange
        var cmd = TestChangePassword.Generate() with { CurrentPassword = "999999" };

        var manager = await InsertManagerAsync(password: cmd.CurrentPassword);

        // Act
        var result = await SendAsync(cmd, manager);

        // Assert
        Assert.IsType<NoContent>(result);
        await ExecuteScopeAsync(async sp =>
        {
            var um = sp.GetRequiredService<UserManager<AppUser>>();
            var user = await um.FindByNameAsync(manager.UserName);
            Assert.True(await um.CheckPasswordAsync(user, cmd.NewPassword));
        });
    }
}
