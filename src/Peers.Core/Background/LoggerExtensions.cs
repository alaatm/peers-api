namespace Peers.Core;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Publishing message of type {MessageType}.", SkipEnabledCheck = true)]
    public static partial void PublishingMessage(this ILogger logger, Type messageType);

    [LoggerMessage(LogLevel.Debug, "Consumer {InstanceId} is starting.", SkipEnabledCheck = true)]
    public static partial void ConsumerStarting(this ILogger logger, int instanceId);

    [LoggerMessage(LogLevel.Information, "Consumer {InstanceId} received message of type {MessageType}.", SkipEnabledCheck = true)]
    public static partial void ConsumerReceivedMessage(this ILogger logger, int instanceId, Type messageType);

    [LoggerMessage(LogLevel.Information, "Consumer {InstanceId} executed message of type {MessageType} in {elapsedMilliseconds}ms.", SkipEnabledCheck = true)]
    public static partial void ConsumerExecutedMessage(this ILogger logger, int instanceId, Type messageType, double elapsedMilliseconds);

    [LoggerMessage(LogLevel.Error, "Consumer {InstanceId} caught an exception.", SkipEnabledCheck = true)]
    public static partial void ConsumerCaughtException(this ILogger logger, Exception ex, int instanceId);

    [LoggerMessage(LogLevel.Debug, "Consumer {InstanceId} forced stop.", SkipEnabledCheck = true)]
    public static partial void ConsumerForcedStop(this ILogger logger, int instanceId);

    [LoggerMessage(LogLevel.Debug, "Consumer {InstanceId} shutting down.", SkipEnabledCheck = true)]
    public static partial void ConsumerShuttingDown(this ILogger logger, int instanceId);
}
