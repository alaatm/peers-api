using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Commands.Responses;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ResetPasswordTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Accepted_with_no_object_when_user_not_found()
    {
        // Arrange and act
        var result = await SendAsync(new ResetPassword.Command("nonexisting@domain.com"));

        // Assert
        Assert.IsType<Accepted>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_password_reset_token()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(new ResetPassword.Command(manager.UserName));

        // Assert
        var accepted = Assert.IsType<Accepted<OtpResponse>>(result);
        var otpResponse = accepted.Value;
        Assert.Equal(manager.UserName, otpResponse.Username);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Emails_password_reset_token()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        string recipent = null;
        OnEmail = (_, _, r_) => recipent = r_;

        // Act
        var result = await SendAsync(new ResetPassword.Command(manager.UserName));

        // Assert
        var accepted = Assert.IsType<Accepted<OtpResponse>>(result);
        var otpResponse = accepted.Value;
        Assert.Equal(manager.UserName, recipent);
    }
}
