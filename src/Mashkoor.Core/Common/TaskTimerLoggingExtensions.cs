namespace Mashkoor.Core.Common;

public static partial class TaskTimerLoggingExtensions
{
    [LoggerMessage(LogLevel.Information, "Startup tasks starting.", SkipEnabledCheck = true)]
    public static partial void StartupTasksStarting(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Startup tasks done in {ElapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void StartupTasksDone(this ILogger logger, double elapsedMilliseconds);

    [LoggerMessage(LogLevel.Information, "Startup task '{Name}' starting.", SkipEnabledCheck = true)]
    public static partial void StartupTaskStarted(this ILogger logger, string name);

    [LoggerMessage(LogLevel.Information, "Startup task '{Name}' finished in {ElapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void StartupTaskFinished(this ILogger logger, string name, double elapsedMilliseconds);

    [LoggerMessage(LogLevel.Critical, "Startup tasks failed.", SkipEnabledCheck = true)]
    public static partial void StartupTasksFailed(this ILogger logger, Exception ex);
}
