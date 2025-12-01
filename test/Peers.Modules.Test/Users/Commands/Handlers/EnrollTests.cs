using Microsoft.AspNetCore.Http.HttpResults;
using Peers.Modules.Users.Events;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class EnrollTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_already_authenticated()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var cmd = TestEnroll().Generate();

        // Act
        var result = await SendAsync(cmd, customer);

        // Assert
        var unauthResult = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("You are already authenticated.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Conflict_when_username_already_exist()
    {
        // Arrange
        var cmd = TestEnroll().Generate();
        await EnrollCustomer(username: cmd.Username);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var conflict = Assert.IsType<Conflict<ProblemDetails>>(result);
        var problem = conflict.Value;
        Assert.Equal("Username or phone number already exist.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Conflict_when_phoneNumber_already_exist()
    {
        // Arrange
        var cmd = TestEnroll().Generate();
        await EnrollCustomer(phoneNumber: cmd.PhoneNumber);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var conflict = Assert.IsType<Conflict<ProblemDetails>>(result);
        var problem = conflict.Value;
        Assert.Equal("Username or phone number already exist.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Publishes_EnrollRequested_event_when_user_doesnt_exist()
    {
        // Arrange
        var cmd = TestEnroll().Generate();

        // Act
        ProducerMoq.Reset();
        var result = await SendAsync(cmd);

        // Assert
        ProducerMoq.Verify(p => p.PublishAsync(It.Is<EnrollRequested>(p =>
            p.Username == cmd.Username &&
            p.PhoneNumber == cmd.PhoneNumber &&
            p.LangCode == cmd.Lang), It.IsAny<CancellationToken>()), Times.Once);
    }
}
