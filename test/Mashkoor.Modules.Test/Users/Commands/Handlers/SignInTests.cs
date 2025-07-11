using Mashkoor.Core.Http;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Events;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class SignInTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_user_doesnt_exist()
    {
        // Arrange
        var cmd = TestSignIn().Generate();

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = badRequest.Value;
        Assert.Equal("Account does not exist.", problem.Detail);

        ProducerMoq.Verify(p => p.PublishAsync(It.Is<SignInRequested>(p =>
            p.Platform == cmd.Platform &&
            p.Username == cmd.PhoneNumber &&
            p.LangCode == cmd.Lang), It.IsAny<CancellationToken>()), Times.Never);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_user_is_deleted()
    {
        // Arrange
        var cmd = TestSignIn().Generate();
        var customer = await EnrollCustomer(cmd.PhoneNumber);
        AssertX.IsType<NoContent>(await SendAsync(new DeleteAccount.Command(), customer));

        // Restore post-fixed username so that we can pass phone number validation for this test
        ExecuteDbContext(db =>
        {
            customer = db.Customers.Include(p => p.User).First(p => p.Id == customer.Id);
            customer.Username = customer.User.UserName = customer.User.NormalizedUserName = customer.User.PhoneNumber = customer.User.OriginalDeletedUsername;
            db.SaveChanges();
        });

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = badRequest.Value;
        Assert.Equal("Account does not exist.", problem.Detail);

        ProducerMoq.Verify(p => p.PublishAsync(It.Is<SignInRequested>(p =>
            p.Platform == cmd.Platform &&
            p.Username == cmd.PhoneNumber &&
            p.LangCode == cmd.Lang), It.IsAny<CancellationToken>()), Times.Never);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Forbidden_when_user_is_banned()
    {
        // Arrange
        var cmd = TestSignIn().Generate();
        await EnrollCustomer(cmd.PhoneNumber, isBanned: true);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Publishes_SignInRequested_event_when_customer_exist_and_returns_accepted()
    {
        // Arrange
        var cmd = TestSignIn().Generate();
        await EnrollCustomer(cmd.PhoneNumber);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        Assert.IsType<Accepted>(result);
        ProducerMoq.Verify(p => p.PublishAsync(It.Is<SignInRequested>(p =>
            p.Platform == cmd.Platform &&
            p.Username == cmd.PhoneNumber &&
            p.LangCode == cmd.Lang), It.IsAny<CancellationToken>()), Times.Once);
    }
}
