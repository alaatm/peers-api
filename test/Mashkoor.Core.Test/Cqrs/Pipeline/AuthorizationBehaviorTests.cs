using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Core.Http;
using Mashkoor.Core.Identity;
using Mashkoor.Resources;
using static Mashkoor.Core.Test.MockBuilder;

namespace Mashkoor.Core.Test.Cqrs.Pipeline;

public class AuthorizationBehaviorTests
{
    private static readonly SLMoq<res> _locMoq = new();
    private const string TestRole1 = "test-role-1";
    private const string TestRole2 = " test-role-2 ";
    private const string TestRole3 = " test-role-3 ";

    [Fact]
    public async Task Handle_executes_next_when_command_has_no_auth_attribute()
    {
        // Arrange
        var nextCalled = false;
        var handler = new AuthorizationBehavior<NoAuth, IResult>(Mock.Of<IIdentityInfo>(), _locMoq);

        // Act
        var result = await handler.Handle(new NoAuth(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var objResult = Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_returns_Unauthorized_when_command_has_auth_attribute_and_user_is_unauthenticated()
    {
        // Arrange
        var nextCalled = false;
        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(false);
        var handler = new AuthorizationBehavior<HasAuthNoRoles, IResult>(identityMoq.Object, _locMoq);

        // Act
        var result = await handler.Handle(new HasAuthNoRoles(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var unauthResult = Assert.IsType<UnauthorizedHttpResult2<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("Authentication required.", problem.Detail);
        Assert.Equal("AUTH_REQUIRED", problem.Type);
        Assert.False(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Handle_executes_next_when_command_has_auth_attribute_but_no_roles_and_user_is_authenticated()
    {
        // Arrange
        var nextCalled = false;
        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        var handler = new AuthorizationBehavior<HasAuthNoRoles, IResult>(identityMoq.Object, _locMoq);

        // Act
        var result = await handler.Handle(new HasAuthNoRoles(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var objResult = Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Handle_returns_Forbidden_when_command_has_auth_attribute_and_roles_and_user_is_authenticated_but_is_not_in_role()
    {
        // Arrange
        var nextCalled = false;
        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.Setup(p => p.IsInRole(TestRole1.Trim())).Returns(false);
        identityMoq.Setup(p => p.IsInRole(TestRole2.Trim())).Returns(false);
        var handler = new AuthorizationBehavior<HasAuthHasRoles, IResult>(identityMoq.Object, _locMoq);

        // Act
        var result = await handler.Handle(new HasAuthHasRoles(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var unauthResult = Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("You are not authorized to perform this operation.", problem.Detail);
        Assert.False(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Handle_executes_next_when_command_has_auth_attribute_and_roles_and_user_is_authenticated_and_is_in_atleast_one_role()
    {
        // Arrange
        var nextCalled = false;
        var identityMoq = new Mock<IIdentityInfo>();
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.Setup(p => p.IsInRole(TestRole1.Trim())).Returns(true);
        var handler = new AuthorizationBehavior<HasAuthHasRoles, IResult>(identityMoq.Object, _locMoq);

        // Act
        var result = await handler.Handle(new HasAuthHasRoles(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var objResult = Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Handle_returns_Forbidden_when_command_has_multi_auth_attribute_and_roles_and_user_is_authenticated_and_is_not_in_all_roles()
    {
        // Arrange
        var nextCalled = false;
        var identityMoq = new Mock<IIdentityInfo>();
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.Setup(p => p.IsInRole(TestRole1.Trim())).Returns(true);
        var handler = new AuthorizationBehavior<HasMultiAuthHasRoles, IResult>(identityMoq.Object, _locMoq);

        // Act
        var result = await handler.Handle(new HasMultiAuthHasRoles(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var unauthResult = Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("You are not authorized to perform this operation.", problem.Detail);
        Assert.False(nextCalled);
        identityMoq.VerifyAll();
    }

    [Fact]
    public async Task Handle_executes_next_when_command_has_multi_auth_attribute_and_roles_and_user_is_authenticated_and_is_in_all_roles()
    {
        // Arrange
        var nextCalled = false;
        var identityMoq = new Mock<IIdentityInfo>();
        identityMoq.SetupGet(p => p.IsAuthenticated).Returns(true);
        identityMoq.Setup(p => p.IsInRole(TestRole1.Trim())).Returns(true);
        identityMoq.Setup(p => p.IsInRole(TestRole2.Trim())).Returns(false);
        identityMoq.Setup(p => p.IsInRole(TestRole3.Trim())).Returns(true);
        var handler = new AuthorizationBehavior<HasMultiAuthHasRoles, IResult>(identityMoq.Object, _locMoq);

        // Act
        var result = await handler.Handle(new HasMultiAuthHasRoles(), (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var objResult = Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
        identityMoq.VerifyAll();
    }

    private record NoAuth() : ICommand;
    [Authorize] private record HasAuthNoRoles() : ICommand;
    [Authorize(Roles = $" {TestRole1},    ,, ,{TestRole2},")] private record HasAuthHasRoles() : ICommand;
    [Authorize(Roles = $" {TestRole1},")][Authorize(Roles = $" {TestRole2},{TestRole3}")] private record HasMultiAuthHasRoles() : ICommand;
}
