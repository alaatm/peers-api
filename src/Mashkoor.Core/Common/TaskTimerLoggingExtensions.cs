namespace Mashkoor.Core.Common;

public static partial class TaskTimerLoggingExtensions
{
    [LoggerMessage(25, LogLevel.Information, "Startup tasks starting.", SkipEnabledCheck = true)]
    public static partial void StartupTasksStarting(this ILogger logger);

    [LoggerMessage(26, LogLevel.Information, "Startup tasks done in {ElapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void StartupTasksDone(this ILogger logger, double elapsedMilliseconds);

    [LoggerMessage(27, LogLevel.Information, "Startup task '{Name}' starting.", SkipEnabledCheck = true)]
    public static partial void StartupTaskStarted(this ILogger logger, string name);

    [LoggerMessage(28, LogLevel.Information, "Startup task '{Name}' finished in {ElapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void StartupTaskFinished(this ILogger logger, string name, double elapsedMilliseconds);

    [LoggerMessage(29, LogLevel.Critical, "Startup tasks failed.", SkipEnabledCheck = true)]
    public static partial void StartupTasksFailed(this ILogger logger, Exception ex);

    [LoggerMessage(LogLevel.Critical, "Storage cleanup after failed startup tasks has failed.", SkipEnabledCheck = true)]
    public static partial void StorageCleanupFailed(this ILogger logger, Exception ex);
}
