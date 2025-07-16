using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace Mashkoor.Core.Communication.Push;

/// <summary>
/// Push notification service.
/// </summary>
public sealed class PushNotificationService : IPushNotificationService
{
    private readonly TimeProvider _timeProvider;
    private readonly IFirebaseMessagingService _firebase;
    private readonly IPushNotificationProblemReporter _pushNotificationProblemReporter;
    private readonly ILogger<PushNotificationService> _log;
    private readonly int _maxRetryCount;
    private readonly int _backoffInMilliseconds;

    public const int DefaultMaxRetryCount = 3;
    public const int DefaultBackoffInMilliseconds = 2000;

    /// <summary>
    /// Creates a new instance of <see cref="PushNotificationService"/>.
    /// </summary>
    /// <param name="timeProvider">The time provider.</param>
    /// <param name="firebase">The firebase service implementation.</param>
    /// <param name="pushNotificationProblemReporter">The problem reporting service.</param>
    /// <param name="log">The logger.</param>
    public PushNotificationService(
        TimeProvider timeProvider,
        IFirebaseMessagingService firebase,
        IPushNotificationProblemReporter pushNotificationProblemReporter,
        ILogger<PushNotificationService> log) : this(timeProvider, firebase, pushNotificationProblemReporter, log, DefaultMaxRetryCount, DefaultBackoffInMilliseconds)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="PushNotificationService"/>.
    /// </summary>
    /// <param name="timeProvider">The time provider.</param>
    /// <param name="firebase">The firebase service implementation.</param>
    /// <param name="pushNotificationProblemReporter">The problem reporting service.</param>
    /// <param name="log">The logger.</param>
    /// <param name="maxRetryCount">The max retry count. Defaults to 3.</param>
    /// <param name="backoffInMilliseconds">The backoff in milliseconds between retries. Defaults to 2000ms.</param>
    public PushNotificationService(
        TimeProvider timeProvider,
        IFirebaseMessagingService firebase,
        IPushNotificationProblemReporter pushNotificationProblemReporter,
        ILogger<PushNotificationService> log,
        int maxRetryCount,
        int backoffInMilliseconds)
    {
        _timeProvider = timeProvider;
        _firebase = firebase;
        _pushNotificationProblemReporter = pushNotificationProblemReporter;
        _log = log;
        _maxRetryCount = maxRetryCount;
        _backoffInMilliseconds = backoffInMilliseconds;
    }

    /// <summary>
    /// Dispatches all specified messages using underlying firebase service.
    /// </summary>
    /// <param name="messageGroup">The message group to dispatch.</param>
    /// <returns></returns>
    public async Task<(IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>)> DispatchAsync((IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>) messageGroup)
    {
        var retries = 0;
        bool hasFailure;

        if (!messageGroup.Item1.Any() && !messageGroup.Item2.Any())
        {
            return messageGroup;
        }

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(_backoffInMilliseconds * retries), _timeProvider);

            _log.PushNotificationDispatch(retries + 1, messageGroup.Item1.Count, messageGroup.Item2.Count, messageGroup.Item2.Sum(p => p.Tokens.Count));
            messageGroup = await DispatchCoreAsync(messageGroup);

            if (hasFailure = messageGroup.Item1.Any() || messageGroup.Item2.Any())
            {
                _log.PushNotificationDispatchFailure(messageGroup.Item1.Count, messageGroup.Item2.Count, messageGroup.Item2.Sum(p => p.Tokens.Count));
            }
        } while (hasFailure && ++retries < _maxRetryCount);

        return messageGroup;
    }

    private async Task<(IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>)> DispatchCoreAsync((IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>) messageGroup)
    {
        var failedMessages = new List<Message>();
        var failedMulticastMessages = new List<MulticastMessage>();
        var (messages, multicastMessages) = messageGroup;
        var dispatchErrors = new List<(string, ErrorCode, MessagingErrorCode?)>();

        if (messages.Any())
        {
            if (messages.Count == 1)
            {
                var response = await _firebase.SendAsync(messages[0]);
                if (!response.IsSuccess)
                {
                    dispatchErrors.Add((messages[0].Token, response.Exception!.ErrorCode, response.Exception!.MessagingErrorCode));
                    _log.PushSendError(messages[0].Token, response.Exception!.ErrorCode, response.Exception!.MessagingErrorCode);

                    if (response.Exception!.ErrorCode is not (ErrorCode.InvalidArgument or ErrorCode.NotFound))
                    {
                        failedMessages.Add(messages[0]);
                    }
                }
            }
            else
            {
                var tasks = messages
                    // The maximum allowed batch size is 500
                    .Chunk(500)
                    .Select(_firebase.SendAllAsync);

                var responses = (await Task.WhenAll(tasks))
                    .SelectMany(p => p.Responses)
                    .ToArray();

                for (var i = 0; i < messages.Count; i++)
                {
                    if (!responses[i].IsSuccess)
                    {
                        dispatchErrors.Add((messages[i].Token, responses[i].Exception!.ErrorCode, responses[i].Exception!.MessagingErrorCode));
                        _log.PushSendError(messages[i].Token, responses[i].Exception!.ErrorCode, responses[i].Exception!.MessagingErrorCode);

                        if (responses[i].Exception!.ErrorCode is not (ErrorCode.InvalidArgument or ErrorCode.NotFound))
                        {
                            failedMessages.Add(messages[i]);
                        }
                    }
                }
            }
        }

        if (multicastMessages.Any())
        {
            var tasks = multicastMessages
                .Select(_firebase.SendMulticastAsync);

            var responses = (await Task.WhenAll(tasks))
                .SelectMany(p => p.Responses)
                .ToArray();

            var prevCount = 0;
            var failedTokens = new List<string>();

            foreach (var mm in multicastMessages)
            {
                var mmResponses = responses[prevCount..(prevCount + mm.Tokens.Count)];
                prevCount += mm.Tokens.Count;

                failedTokens.Clear();

                for (var t = 0; t < mm.Tokens.Count; t++)
                {
                    if (!mmResponses[t].IsSuccess)
                    {
                        dispatchErrors.Add((mm.Tokens[t], mmResponses[t].Exception!.ErrorCode, mmResponses[t].Exception!.MessagingErrorCode));
                        _log.PushSendError(mm.Tokens[t], mmResponses[t].Exception!.ErrorCode, mmResponses[t].Exception!.MessagingErrorCode);

                        if (mmResponses[t].Exception!.ErrorCode is not (ErrorCode.InvalidArgument or ErrorCode.NotFound))
                        {
                            failedTokens.Add(mm.Tokens[t]);
                        }
                    }
                }

                if (failedTokens.Count != 0)
                {
                    failedMulticastMessages.Add(CopyMessage(mm, failedTokens));
                }
            }
        }

        if (dispatchErrors.Count != 0)
        {
            await _pushNotificationProblemReporter.ReportErrorsAsync(dispatchErrors);
        }

        return (failedMessages, failedMulticastMessages);

        static MulticastMessage CopyMessage(MulticastMessage mm, List<string> tokens) => new()
        {
            Android = mm.Android,
            Apns = mm.Apns,
            Data = mm.Data,
            Notification = mm.Notification,
            Webpush = mm.Webpush,
            Tokens = [.. tokens],
        };
    }
}
