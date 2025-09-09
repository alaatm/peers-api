using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Commands.Responses;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ResetPasswordConfirmTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_user_not_found()
    {
        // Arrange and act
        var result = await SendAsync(TestResetPasswordConfirm);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("User does not exist.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadResult_when_invalid_token()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(TestResetPasswordConfirm with { Username = manager.UserName });

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Password reset failed", problem.Detail);
        var errors = Assert.IsType<string[]>(problem.Extensions["errors"]);
        Assert.Equal("Invalid token.", errors[0]);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Resets_password()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        string otp = null;
        OnEmail = (_, otp_, _) => otp = otp_.Split(':')[1].Trim();

        var result = Assert.IsType<Accepted<OtpResponse>>(await SendAsync(new ResetPassword.Command(manager.UserName)));
        var cmd = TestResetPasswordConfirm with { Otp = otp, Username = manager.UserName };

        // Act
        var result2 = await SendAsync(cmd);

        // Assert
        AssertX.IsType<NoContent>(result2);
        await ExecuteScopeAsync(async sp =>
        {
            var um = sp.GetRequiredService<UserManager<AppUser>>();
            var user = await um.FindByNameAsync(manager.UserName);
            Assert.True(await um.CheckPasswordAsync(user, cmd.NewPassword));
        });
    }

    private static ResetPasswordConfirm.Command TestResetPasswordConfirm => new("123456", "email@contoso.com", "P@ssword");
}
