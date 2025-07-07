using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Cqrs;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Core.Identity;

namespace Mashkoor.Core.Test.Cqrs;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCqrs_adds_all_required_service()
    {
        // Arrange and act
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .AddIdentityInfo()
            .AddCqrs((_) => { }, Assembly.GetExecutingAssembly())
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IMediator>();
        serviceProvider.GetRequiredService<IValidator<CqrsTests_TestCommand>>();

        var pipelineServices = serviceProvider.GetServices<IPipelineBehavior<CqrsTests_TestCommand, IResult>>().TakeLast(3).ToArray();
        Assert.IsType<LoggingBehavior<CqrsTests_TestCommand, IResult>>(pipelineServices[0]);
        Assert.IsType<AuthorizationBehavior<CqrsTests_TestCommand, IResult>>(pipelineServices[1]);
        Assert.IsType<CommandValidationBehavior<CqrsTests_TestCommand, IResult>>(pipelineServices[2]);
    }

    public record CqrsTests_TestCommand : ICommand, IValidatable;
    public class CqrsTests_TestCommandValidator : AbstractValidator<CqrsTests_TestCommand> { }
}
