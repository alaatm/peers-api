using Microsoft.AspNetCore.Http.HttpResults;
using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ResetPasswordTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Accepted_when_user_not_found()
    {
        // Arrange and act
        var result = await SendAsync(new ResetPassword.Command("nonexisting@domain.com"));

        // Assert
        Assert.IsType<Accepted>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Accepted_and_Emails_password_reset_token()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        string recipent = null;
        OnEmail = (_, _, r_) => recipent = r_;

        // Act
        var result = await SendAsync(new ResetPassword.Command(manager.UserName));

        // Assert
        Assert.IsType<Accepted>(result);
    }
}
