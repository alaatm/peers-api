using MediatR;
using System.Diagnostics.CodeAnalysis;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Domain;
using Mashkoor.Core.Http;
using System.Diagnostics;

namespace Mashkoor.Core.Cqrs.Pipeline;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand, IRequest<TResponse>
    where TResponse : IResult
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, [NotNull] RequestHandlerDelegate<TResponse> next, CancellationToken ctk = default)
    {
        var requestType = typeof(TRequest);
        TResponse? response = default;

        _logger.HandlingRequestDetailed(requestType, request?.ToString() ?? "null");
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            response = await next(ctk);
        }
        catch (BusinessRuleValidationException ex)
        {
            _logger.BusinessRulesException(ex);
            return (TResponse)Result.BadRequest(detail: ex.BrokenRule.ErrorTitle, type: ex.BrokenRule.Code, errors: [.. ex.BrokenRule.Errors]);
        }
        finally
        {
            _logger.HandledRequest(requestType, response?.GetType().Name ?? "ERR", Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
        }

        return response;
    }
}
