using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Peers.Core.Commands;
using Peers.Core.Http;
using Peers.Core.Identity;
using Peers.Core.Localization;
using Peers.Modules.Kernel.Pipelines;
using Peers.Modules.Test.SharedClasses;
using Peers.Modules.Users.Domain;
using static Peers.Modules.Test.SharedClasses.MockBuilder;

namespace Peers.Modules.Test.Kernel.Pipelines;

// Must be in collection for shared IdentityCheckBehavior enabled flag
[Collection(nameof(IntegrationTestBaseCollection))]
public sealed class IdentityCheckBehaviorTests : IDisposable
{
    public IdentityCheckBehaviorTests() => IdentityCheckBehaviorOptions.Enabled = true;
    public void Dispose() => IdentityCheckBehaviorOptions.Enabled = false;

    [Fact]
    public async Task Executes_next_when_not_authenticated()
    {
        // Arrange
        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(false);

        var nextCalled = false;
        var handler = new IdentityCheckBehavior<TestCommand, IResult>(
            null,
            identityMoq.Object,
            new Logger(),
            new SLMoq<res>());

        // Act
        var result = await handler.Handle(new TestCommand(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Executes_next_when_authenticated_and_user_exist()
    {
        // Arrange
        var context = InMemoryPeersContext.Create();
        var user = AppUser.CreatePasswordAccount(DateTime.UtcNow, "user@email.com", "John", "Doe", Lang.EnLangCode);
        context.Users.Add(user);
        context.SaveChanges();

        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.SetupGet(p => p.Id).Returns(user.Id);

        var nextCalled = false;
        var handler = new IdentityCheckBehavior<TestCommand, IResult>(
            BuildServiceProvider(context),
            identityMoq.Object,
            new Logger(),
            new SLMoq<res>());

        // Act
        var result = await handler.Handle(new TestCommand(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Returns_ServerError_when_authenticated_and_user_does_not_exist()
    {
        // Arrange
        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.SetupGet(p => p.Id).Returns(1);
        identityMoq.SetupGet(p => p.Username).Returns("user@email.com");

        var context = InMemoryPeersContext.Create();

        var nextCalled = false;
        var handler = new IdentityCheckBehavior<TestCommand, IResult>(
            BuildServiceProvider(context),
            identityMoq.Object,
            new Logger(),
            new SLMoq<res>());

        // Act
        var result = await handler.Handle(new TestCommand(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal("Unexpected state.", problem.ProblemDetails.Detail);
        Assert.False(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Returns_Forbidden_when_auth_attribute_and_banned_user()
    {
        // Arrange
        var user = Test2FUser(isBanned: true).Generate();

        var context = InMemoryPeersContext.Create();
        context.Users.Add(user);
        context.SaveChanges();

        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.SetupGet(p => p.Id).Returns(user.Id);
        identityMoq.SetupGet(p => p.Username).Returns(user.UserName);

        var nextCalled = false;
        var handler = new IdentityCheckBehavior<TestCommandWithAuth, IResult>(
            BuildServiceProvider(context),
            identityMoq.Object,
            new LoggerWithAuth(),
            new SLMoq<res>());

        // Act
        var result = await handler.Handle(new TestCommandWithAuth(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var forbiddenResult = Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(result);
        Assert.Equal("USER_BANNED", forbiddenResult.Value.Type);
        Assert.False(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Returns_Forbidden_when_no_auth_attribute_and_banned_user()
    {
        // Arrange
        var user = Test2FUser(isBanned: true).Generate();

        var context = InMemoryPeersContext.Create();
        context.Users.Add(user);
        context.SaveChanges();

        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.SetupGet(p => p.Id).Returns(user.Id);
        identityMoq.SetupGet(p => p.Username).Returns(user.UserName);

        var nextCalled = false;
        var handler = new IdentityCheckBehavior<TestCommandNoAuth, IResult>(
            BuildServiceProvider(context),
            identityMoq.Object,
            new LoggerNoAuth(),
            new SLMoq<res>());

        // Act
        var result = await handler.Handle(new TestCommandNoAuth(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var forbiddenResult = Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(result);
        Assert.Equal("USER_BANNED", forbiddenResult.Value.Type);
        Assert.False(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Executes_next_when_no_auth_attribute_and_non_authenticated_user()
    {
        // Arrange
        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(false);

        var nextCalled = false;
        var handler = new IdentityCheckBehavior<TestCommandNoAuth, IResult>(
            BuildServiceProvider(InMemoryPeersContext.Create()),
            identityMoq.Object,
            new LoggerNoAuth(),
            new SLMoq<res>());

        // Act
        var result = await handler.Handle(new TestCommandNoAuth(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Executes_next_when_auth_attribute_and_non_banned_user()
    {
        // Arrange
        var user = Test2FUser().Generate();

        var context = InMemoryPeersContext.Create();
        context.Users.Add(user);
        context.SaveChanges();

        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.SetupGet(p => p.Id).Returns(user.Id);

        var nextCalled = false;
        var handler = new IdentityCheckBehavior<TestCommandWithAuth, IResult>(
            BuildServiceProvider(context),
            identityMoq.Object,
            new LoggerWithAuth(),
            new SLMoq<res>());

        // Act
        var result = await handler.Handle(new TestCommandWithAuth(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
        identityMoq.VerifyAll();
    }

    private record TestCommand() : ICommand;

    private class Logger : NullLogger<IdentityCheckBehavior<TestCommand, IResult>>
    {
    }

    private record TestCommandNoAuth() : ICommand;

    [Authorize]
    private record TestCommandWithAuth() : ICommand;

    private class LoggerNoAuth : NullLogger<IdentityCheckBehavior<TestCommandNoAuth, IResult>>
    {
    }

    private class LoggerWithAuth : NullLogger<IdentityCheckBehavior<TestCommandWithAuth, IResult>>
    {
    }

    private static ServiceProvider BuildServiceProvider(PeersContext context)
        => new ServiceCollection()
            .AddScoped(_ => context)
            .BuildServiceProvider();
}
