using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Peers.Core.Commands;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Http;
using Peers.Resources;
using static Peers.Core.Test.MockBuilder;

namespace Peers.Core.Test.Cqrs.Pipeline;

public class CommandValidationBehaviorTests
{
    private static readonly SLMoq<res> _locMoq = new();

    [Fact]
    public async Task Handle_returns_ValidationProblem_when_validation_fails()
    {
        // Arrange
        var nextCalled = false;
        var cmd = new TestCommand("");
        var handler = new CommandValidationBehavior<TestCommand, IResult>(new TestCommandValidator(), Mock.Of<ILogger<CommandValidationBehavior<TestCommand, IResult>>>(), _locMoq);

        // Act
        var result = await handler.Handle(cmd, (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        var validationProblem = Assert.IsType<HttpValidationProblemDetails>(problem.ProblemDetails);
        var error = Assert.Single(validationProblem.Errors);
        Assert.Equal("Name", error.Key);
        Assert.Equal(2, error.Value.Length);
        Assert.Equal("'Name' must not be empty.", error.Value[0]);
        Assert.Equal("'Name' must be 5 characters in length. You entered 0 characters.", error.Value[1]);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Handle_executes_next_pipeline_when_validation_succeeds()
    {
        // Arrange
        var nextCalled = false;
        var cmd = new TestCommand("12345");
        var handler = new CommandValidationBehavior<TestCommand, IResult>(new TestCommandValidator(), Mock.Of<ILogger<CommandValidationBehavior<TestCommand, IResult>>>(), _locMoq);

        // Act
        var result = await handler.Handle(cmd, (_) => { nextCalled = true; return Task.FromResult(Result.Ok()); });

        // Assert
        var objResult = Assert.IsType<Ok>(result);
        Assert.True(nextCalled);
    }

    public record TestCommand(string Name) : ICommand, IValidatable;

    private class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
            => RuleFor(p => p.Name).NotEmpty().Length(5);
    }
}
