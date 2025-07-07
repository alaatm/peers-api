namespace Mashkoor.Core;

internal static partial class LoggerExtensions
{
    [LoggerMessage(2001, LogLevel.Information, "Started handling request {RequestType}.", SkipEnabledCheck = true)]
    public static partial void HandlingRequest(this ILogger logger, Type requestType);

    [LoggerMessage(2002, LogLevel.Information, "Started handling request {RequestType} with data {RequestObject}.", SkipEnabledCheck = true)]
    public static partial void HandlingRequestDetailed(this ILogger logger, Type requestType, string requestObject);

    [LoggerMessage(2003, LogLevel.Information, "Finished handling request {RequestType} with response {Response} in {ElapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void HandledRequest(this ILogger logger, Type requestType, string response, double elapsedMilliseconds);

    [LoggerMessage(2004, LogLevel.Warning, "A business rule exception was thrown while handling request.", SkipEnabledCheck = true)]
    public static partial void BusinessRulesException(this ILogger logger, Exception ex);

    [LoggerMessage(2005, LogLevel.Warning, "Command validation errors: {Errors} were caught while handling request.", SkipEnabledCheck = true)]
    public static partial void CommandValidationError(this ILogger logger, string errors);
}
