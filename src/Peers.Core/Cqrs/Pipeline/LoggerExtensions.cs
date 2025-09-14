namespace Peers.Core;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Started handling request {RequestType}.", SkipEnabledCheck = true)]
    public static partial void HandlingRequest(this ILogger logger, Type requestType);

    [LoggerMessage(LogLevel.Information, "Started handling request {RequestType} with data {RequestObject}.", SkipEnabledCheck = true)]
    public static partial void HandlingRequestDetailed(this ILogger logger, Type requestType, string requestObject);

    [LoggerMessage(LogLevel.Information, "Finished handling request {RequestType} with response {Response} in {ElapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void HandledRequest(this ILogger logger, Type requestType, string response, double elapsedMilliseconds);

    [LoggerMessage(LogLevel.Information, "A business rule exception was thrown while handling request.", SkipEnabledCheck = true)]
    public static partial void BusinessRulesException(this ILogger logger, Exception ex);

    [LoggerMessage(LogLevel.Information, "Command validation errors: {Errors} were caught while handling request.", SkipEnabledCheck = true)]
    public static partial void CommandValidationError(this ILogger logger, string errors);
}
