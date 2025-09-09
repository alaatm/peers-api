using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using MediatR;
using Peers.Core.Commands;
using Peers.Core.Http;

namespace Peers.Core.Cqrs.Pipeline;

/// <summary>
/// Represents a command or query that must be validated.
/// </summary>
public interface IValidatable { }

public sealed class CommandValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand, IRequest<TResponse>, IValidatable
    where TResponse : IResult
{
    private readonly IValidator<TRequest> _validator;
    private readonly ILogger<CommandValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IStrLoc _l;

    public CommandValidationBehavior(
        IValidator<TRequest> validator,
        ILogger<CommandValidationBehavior<TRequest, TResponse>> logger,
        IStrLoc l)
    {
        _validator = validator;
        _logger = logger;
        _l = l;
    }

    public async Task<TResponse> Handle(TRequest cmd, [NotNull] RequestHandlerDelegate<TResponse> next, CancellationToken ctk = default)
    {
        var context = new ValidationContext<TRequest>(cmd);
        var result = await _validator.ValidateAsync(context, ctk);

        if (!result.IsValid)
        {
            var groupedErrors = result.Errors
                .GroupBy(p => p.PropertyName)
                .Select(g => new
                {
                    PropertyName = g.Key,
                    Errors = g.Select(p => p.ErrorMessage).ToArray(),
                })
                .ToArray();

            var errors = new Dictionary<string, string[]>();
            foreach (var error in groupedErrors)
            {
                errors[error.PropertyName] = error.Errors;
            }

            _logger.CommandValidationError(string.Join(',', errors.SelectMany(p => p.Value)));
            return (TResponse)Result.ValidationProblem(errors, detail: _l["One or more validation errors occurred."]);
        }

        return await next(ctk);
    }
}
