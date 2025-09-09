using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Peers.Core;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(1, LogLevel.Error, "Email send error.", SkipEnabledCheck = true)]
    public static partial void EmailSendError(this ILogger logger, Exception ex);

    [LoggerMessage(66367, LogLevel.Warning, "Email will not be sent because the service is disabled.", SkipEnabledCheck = true)]
    public static partial void EmailServiceDisabled(this ILogger logger);

    [LoggerMessage(2500, LogLevel.Information, "Retry {AttemptCount}: dispatching {MessageCount} message(s) and {MulticastMessageCount} multicast message(s) ({MulticastTokenCount} tokens) as push notification(s).", SkipEnabledCheck = true)]
    public static partial void PushNotificationDispatch(this ILogger logger, int attemptCount, int messageCount, int multicastMessageCount, int multicastTokenCount);

    [LoggerMessage(2501, LogLevel.Warning, "Failed to dispatch {FailedMessageCount} message(s) and {FailedMulticastMessageCount} multicast message(s) ({FailedMulticastTokenCount} tokens) as push notification(s).", SkipEnabledCheck = true)]
    public static partial void PushNotificationDispatchFailure(this ILogger logger, int failedMessageCount, int failedMulticastMessageCount, int failedMulticastTokenCount);

    [LoggerMessage(2502, LogLevel.Warning, "Failed to dispatch push notification. Token: {Token}, ErrorCode: {ErrorCode}, MessagingErrorCode: {MessagingErrorCode}.", SkipEnabledCheck = true)]
    public static partial void PushSendError(this ILogger logger, string token, FirebaseAdmin.ErrorCode errorCode, FirebaseAdmin.Messaging.MessagingErrorCode? messagingErrorCode);

    [LoggerMessage(66350, LogLevel.Information, "Sending SMS to {recipient}.", SkipEnabledCheck = true)]
    public static partial void SmsSendRequest(this ILogger logger, string recipient);

    [LoggerMessage(66351, LogLevel.Information, "Successfully sent SMS to {recipient}.", SkipEnabledCheck = true)]
    public static partial void SmsSendRequestSuccess(this ILogger logger, string recipient);

    [LoggerMessage(66352, LogLevel.Error, "SMS send failed with status: {statusCode}, and message: {message}.", SkipEnabledCheck = true)]
    public static partial void SmsSendRequestFail(this ILogger logger, HttpStatusCode statusCode, string? message);

    [LoggerMessage(66357, LogLevel.Warning, "SMS message will not be sent because the service is disabled.", SkipEnabledCheck = true)]
    public static partial void SmsServiceDisabled(this ILogger logger);
}
