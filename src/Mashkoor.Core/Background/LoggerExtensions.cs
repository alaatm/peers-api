namespace Mashkoor.Core;

internal static partial class LoggerExtensions
{
    [LoggerMessage(501, LogLevel.Information, "Publishing message of type {MessageType}.", SkipEnabledCheck = true)]
    public static partial void PublishingMessage(this ILogger logger, Type messageType);

    [LoggerMessage(502, LogLevel.Debug, "Consumer {InstanceId} is starting.", SkipEnabledCheck = true)]
    public static partial void ConsumerStarting(this ILogger logger, int instanceId);

    [LoggerMessage(503, LogLevel.Information, "Consumer {InstanceId} received message of type {MessageType}.", SkipEnabledCheck = true)]
    public static partial void ConsumerReceivedMessage(this ILogger logger, int instanceId, Type messageType);

    [LoggerMessage(504, LogLevel.Information, "Consumer {InstanceId} executed message of type {MessageType} in {elapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void ConsumerExecutedMessage(this ILogger logger, int instanceId, Type messageType, double elapsedMilliseconds);

    [LoggerMessage(550, LogLevel.Error, "Consumer {InstanceId} caught an exception.", SkipEnabledCheck = true)]
    public static partial void ConsumerCaughtException(this ILogger logger, Exception ex, int instanceId);

    [LoggerMessage(505, LogLevel.Debug, "Consumer {InstanceId} forced stop.", SkipEnabledCheck = true)]
    public static partial void ConsumerForcedStop(this ILogger logger, int instanceId);

    [LoggerMessage(506, LogLevel.Debug, "Consumer {InstanceId} shutting down.", SkipEnabledCheck = true)]
    public static partial void ConsumerShuttingDown(this ILogger logger, int instanceId);
}
