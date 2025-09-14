namespace Peers.Core;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "{Name} job scheduled to run in {Delay} at {ScheduledTime} UTC.", SkipEnabledCheck = true)]
    public static partial void JobScheduled(this ILogger logger, string name, TimeSpan delay, DateTime scheduledTime);

    [LoggerMessage(LogLevel.Information, "{Name} job started.", SkipEnabledCheck = true)]
    public static partial void JobStarted(this ILogger logger, string name);

    [LoggerMessage(LogLevel.Information, "{Name} job finished in {elapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void JobFinished(this ILogger logger, string name, double elapsedMilliseconds);

    [LoggerMessage(LogLevel.Information, "{Name} job cron updated from {oldCron} to {newCron}.", SkipEnabledCheck = true)]
    public static partial void JobCronUpdated(this ILogger logger, string name, string oldCron, string newCron);

    [LoggerMessage(LogLevel.Error, "{Name} job caught an exception.", SkipEnabledCheck = true)]
    public static partial void JobCaughtException(this ILogger logger, Exception ex, string name);

    [LoggerMessage(LogLevel.Warning, "{Name} job run time could not be determined.", SkipEnabledCheck = true)]
    public static partial void JobNotRun(this ILogger logger, string name);
}
