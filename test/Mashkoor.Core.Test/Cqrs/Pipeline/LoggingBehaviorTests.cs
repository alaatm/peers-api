using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Core.Domain;
using Mashkoor.Core.Domain.Rules;
using Mashkoor.Core.Http;

namespace Mashkoor.Core.Test.Cqrs.Pipeline;

public class LoggingBehaviorTests
{
    [Fact]
    public async Task Returns_result_of_command_execution()
    {
        // Arrange
        var logMoq = new Mock<ILogger<LoggingBehavior<TestCommand, IResult>>>();
        var handler = new LoggingBehavior<TestCommand, IResult>(logMoq.Object);
        var exception = new BusinessRuleValidationException(new BrokenRule());

        // Act
        var result = await handler.Handle(new TestCommand(), (_) => Task.FromResult(Result.Ok(new TestResponse("test"))));

        // Assert
        var okResult = Assert.IsType<Ok<TestResponse>>(result);
        var response = okResult.Value;
        Assert.Equal("test", response.Title);

        Assert.Equal(2, logMoq.Invocations.Count);
        Assert.Equal(LogLevel.Information, logMoq.Invocations[0].Arguments[0]);
        Assert.Equal("HandlingRequestDetailed", ((EventId)logMoq.Invocations[0].Arguments[1]).Name);
        Assert.Equal(LogLevel.Information, logMoq.Invocations[1].Arguments[0]);
        Assert.Equal("HandledRequest", ((EventId)logMoq.Invocations[1].Arguments[1]).Name);
    }

    [Fact]
    public async Task Handles_exceptions_of_type_BusinessRuleValidationException()
    {
        // Arrange
        var logMoq = new Mock<ILogger<LoggingBehavior<TestCommand, IResult>>>();
        var handler = new LoggingBehavior<TestCommand, IResult>(logMoq.Object);
        var exception = new BusinessRuleValidationException(new BrokenRule());

        // Act
        var result = await handler.Handle(new TestCommand(), (_) => throw exception);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("error title", problem.Detail);
        Assert.Equal(exception.BrokenRule.Errors, problem.Extensions["errors"]);
        Assert.Equal("TEST_CODE", problem.Type);

        Assert.Equal(3, logMoq.Invocations.Count);
        Assert.Equal(LogLevel.Information, logMoq.Invocations[0].Arguments[0]);
        Assert.Equal("HandlingRequestDetailed", ((EventId)logMoq.Invocations[0].Arguments[1]).Name);
        Assert.Equal(LogLevel.Warning, logMoq.Invocations[1].Arguments[0]);
        Assert.Equal("BusinessRulesException", ((EventId)logMoq.Invocations[1].Arguments[1]).Name);
        Assert.Equal(LogLevel.Information, logMoq.Invocations[2].Arguments[0]);
        Assert.Equal("HandledRequest", ((EventId)logMoq.Invocations[2].Arguments[1]).Name);
    }

    [Fact]
    public async Task Does_not_handle_exceptions_other_than_BusinessRuleValidationException()
    {
        // Arrange
        var logMoq = new Mock<ILogger<LoggingBehavior<TestCommand, IResult>>>();
        var handler = new LoggingBehavior<TestCommand, IResult>(logMoq.Object);

        // Act and assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new TestCommand(), (_) => throw new InvalidOperationException("test")));
        Assert.Equal("test", ex.Message);

        Assert.Equal(2, logMoq.Invocations.Count);
        Assert.Equal(LogLevel.Information, logMoq.Invocations[0].Arguments[0]);
        Assert.Equal("HandlingRequestDetailed", ((EventId)logMoq.Invocations[0].Arguments[1]).Name);
        Assert.Equal(LogLevel.Information, logMoq.Invocations[1].Arguments[0]);
        Assert.Equal("HandledRequest", ((EventId)logMoq.Invocations[1].Arguments[1]).Name);
    }

    public record TestCommand() : ICommand;
    public record TestResponse(string Title);
    private class BrokenRule : BusinessRule
    {
        public override string ErrorTitle => "error title";
        public BrokenRule()
        {
            Append("Error1");
            Append("Error2");
            Code = "TEST_CODE";
        }

        public override bool IsBroken() => throw new NotImplementedException();
    }
}
