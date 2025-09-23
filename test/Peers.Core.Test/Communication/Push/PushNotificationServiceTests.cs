using System.Reflection;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Peers.Core.Communication.Push;
using Microsoft.Extensions.Time.Testing;

namespace Peers.Core.Test.Communication.Push;

public class PushNotificationServiceTests
{
    [Fact]
    public async Task DispatchAsync_returns_original_messages_when_empty()
    {
        // Arrange
        var pushService = new PushNotificationService(TimeProvider.System, Mock.Of<IFirebaseMessagingService>(), Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 1, 1);

        var messages = new List<Message>();
        var multicastMessages = new List<MulticastMessage>();

        // Act
        var retVal = await pushService.DispatchAsync((messages, multicastMessages));

        // Assert
        Assert.Same(messages, retVal.Item1);
        Assert.Same(multicastMessages, retVal.Item2);
    }

    [Fact]
    public async Task DispatchAsync_retries_failed_operations_as_specified_in_maxRetryCount()
    {
        // Arrange
        var maxRetryCount = 5;
        var firebaseMoq = new FailingFirebaseMoq();
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), maxRetryCount, 1);

        var messages = new List<Message> { new() { Notification = new() { Body = "Test" }, Token = "111" } };
        var multicastMessages = new List<MulticastMessage> { new() { Notification = new() { Body = "Test" }, Tokens = ["111", "222"] } };

        // Act
        await pushService.DispatchAsync((messages, multicastMessages));

        // Assert
        Assert.Equal(maxRetryCount, firebaseMoq.SendAsyncCallCount);
        Assert.Equal(maxRetryCount, firebaseMoq.SendMulticastAsyncCallCount);
    }

    [Fact]
    public async Task DispatchAsync_backsoff_between_retries()
    {
        // Arrange
        var timeProviderMoq = new FakeTimeProvider();
        var maxRetryCount = 3;
        var backoff = 2000;
        var firebaseMoq = new FailingFirebaseMoq();
        var pushService = new PushNotificationService(timeProviderMoq, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), maxRetryCount, backoff);

        var messages = new List<Message> { new() { Notification = new() { Body = "Test" }, Token = "111" } };
        var multicastMessages = new List<MulticastMessage> { new() { Notification = new() { Body = "Test" }, Tokens = ["111", "222"] } };

        // Act
        var t = pushService.DispatchAsync((messages, multicastMessages));

        // Fast-forward time enough to let retries proceed
        for (var i = 0; i < maxRetryCount; i++)
        {
            // // Give continuation a chance to run
            await Task.Yield();
            await Task.Delay(1);
            timeProviderMoq.Advance(TimeSpan.FromMilliseconds(backoff * i));
        }

        await t;
        Assert.Equal(maxRetryCount, firebaseMoq.SendAsyncCallCount);
        Assert.Equal(maxRetryCount, firebaseMoq.SendMulticastAsyncCallCount);
    }

    [Fact]
    public async Task DispatchAsync_returns_failed_messages()
    {
        // Arrange
        var firebaseMoq = new FailingFirebaseMoq();
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 0, 0);

        var messagesSingle = new List<Message> { new() { Notification = new() { Body = "Test" }, Token = "111" } };
        var messagesMulti = new List<Message> { new() { Notification = new() { Body = "Test" }, Token = "111" }, new() { Notification = new() { Body = "Test" }, Token = "222" } };
        var multicastMessages = new List<MulticastMessage> { new() { Notification = new() { Body = "Test" }, Tokens = ["333", "444"] } };

        // Act
        var failed1 = await pushService.DispatchAsync((messagesSingle, multicastMessages));
        var failed2 = await pushService.DispatchAsync((messagesMulti, multicastMessages));

        // Assert
        Assert.Equal(["111"], failed1.Item1.Select(p => p.Token));
        Assert.Equal(["333", "444"], failed1.Item2.SelectMany(p => p.Tokens));

        Assert.Equal(["111", "222"], failed2.Item1.Select(p => p.Token));
        Assert.Equal(["333", "444"], failed2.Item2.SelectMany(p => p.Tokens));
    }

    [Fact]
    public async Task DispatchAsync_reports_failed_messages_single()
    {
        // Arrange
        var firebaseMoq = new FailingFirebaseMoq();
        var pushNotificationProblemReporterMoq = new Mock<IPushNotificationProblemReporter>();
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, pushNotificationProblemReporterMoq.Object, Mock.Of<ILogger<PushNotificationService>>(), 0, 0);

        var messagesSingle = new List<Message> { new() { Notification = new() { Body = "Test" }, Token = "111" } };
        var multicastMessages = new List<MulticastMessage> { new() { Notification = new() { Body = "Test" }, Tokens = ["333", "444"] } };

        pushNotificationProblemReporterMoq
            .Setup(p => p.ReportErrorsAsync(It.IsAny<List<(string, ErrorCode, MessagingErrorCode?)>>()))
            .Callback<List<(string, ErrorCode, MessagingErrorCode?)>>(arg =>
            {
                Assert.Equal(3, arg.Count);
                Assert.Equal("111", arg.ElementAt(0).Item1);
                Assert.Equal("333", arg.ElementAt(1).Item1);
                Assert.Equal("444", arg.ElementAt(2).Item1);

                Assert.All(arg, p => Assert.Equal(ErrorCode.Unknown, p.Item2));
                Assert.All(arg, p => Assert.Equal(MessagingErrorCode.Internal, p.Item3));
            });

        // Act
        await pushService.DispatchAsync((messagesSingle, multicastMessages));
    }

    [Fact]
    public async Task DispatchAsync_reports_failed_messages_multi()
    {
        // Arrange
        var firebaseMoq = new FailingFirebaseMoq();
        var pushNotificationProblemReporterMoq = new Mock<IPushNotificationProblemReporter>();
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, pushNotificationProblemReporterMoq.Object, Mock.Of<ILogger<PushNotificationService>>(), 0, 0);

        var messagesMulti = new List<Message> { new() { Notification = new() { Body = "Test" }, Token = "111" }, new() { Notification = new() { Body = "Test" }, Token = "222" } };
        var multicastMessages = new List<MulticastMessage> { new() { Notification = new() { Body = "Test" }, Tokens = ["333", "444"] } };

        pushNotificationProblemReporterMoq
            .Setup(p => p.ReportErrorsAsync(It.IsAny<List<(string, ErrorCode, MessagingErrorCode?)>>()))
            .Callback<List<(string, ErrorCode, MessagingErrorCode?)>>(arg =>
            {
                Assert.Equal(4, arg.Count);
                Assert.Equal("111", arg.ElementAt(0).Item1);
                Assert.Equal("222", arg.ElementAt(1).Item1);
                Assert.Equal("333", arg.ElementAt(2).Item1);
                Assert.Equal("444", arg.ElementAt(3).Item1);

                Assert.All(arg, p => Assert.Equal(ErrorCode.Unknown, p.Item2));
                Assert.All(arg, p => Assert.Equal(MessagingErrorCode.Internal, p.Item3));
            });

        // Act
        await pushService.DispatchAsync((messagesMulti, multicastMessages));
    }

    [Fact]
    public async Task Single_message_dispatch_success_on_first_attempt()
    {
        // Arrange
        var firebaseMoq = new PartiallySucceedingFirebaseMoq(0);
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 5, 0);

        var message = new Message { Notification = new() { Body = "Test" }, Token = "111" };

        // Act
        var failed = await pushService.DispatchAsync(([message], []));

        // Assert
        Assert.Empty(failed.Item1);
        Assert.Empty(failed.Item2);
        Assert.Equal(1, firebaseMoq.SendAsyncCallCount);
    }

    [Fact]
    public async Task Single_message_dispatch_success_on_3rd_attempt()
    {
        // Arrange
        var firebaseMoq = new PartiallySucceedingFirebaseMoq(2);
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 5, 0);

        var message = new Message { Notification = new() { Body = "Test" }, Token = "111" };

        // Act
        var failed = await pushService.DispatchAsync(([message], []));

        // Assert
        Assert.Empty(failed.Item1);
        Assert.Empty(failed.Item2);
        Assert.Equal(3, firebaseMoq.SendAsyncCallCount);
    }

    [Fact]
    public async Task Single_message_dispatch_non_success()
    {
        // Arrange
        var firebaseMoq = new PartiallySucceedingFirebaseMoq(999);
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 5, 0);

        var message = new Message { Notification = new() { Body = "Test" }, Token = "111" };

        // Act
        var failed = await pushService.DispatchAsync(([message], []));

        // Assert
        Assert.Single(failed.Item1);
        Assert.Empty(failed.Item2);
        Assert.Equal(5, firebaseMoq.SendAsyncCallCount);
    }

    [Fact]
    public async Task DispatchAsync_returns_failed_messages_on_partial_failure1()
    {
        // Arrange
        var firebaseMoq = new PartiallySucceedingFirebaseMoq(0);
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 3, 0);

        var messages = Enumerable.Range(0, 10).Select(i => new Message { Notification = new() { Body = "Test" }, Token = $"{i:00}" }).ToList();
        var multicastMessages = new List<MulticastMessage>
        {
            new()
            {
                Notification = new() { Body = "Test-1" },
                Tokens = [.. Enumerable.Range(0, 10).Select(i => $"{i:000}")],
            },
            new()
            {
                Notification = new() { Body = "Test-2" },
                Tokens = [.. Enumerable.Range(0, 17).Select(i => $"{i:0000}")],
            },
            new()
            {
                Notification = new() { Body = "Test-3" },
                Tokens = [.. Enumerable.Range(0, 5).Select(i => $"{i:00000}")],
            }
        };

        // Act
        var failed = await pushService.DispatchAsync((messages, multicastMessages));

        // Assert

        // 1st run failures (msg list ): 01,  03,  05,  07,  09
        // 1st run failures (multicast): 001, 003, 005, 007, 009 - 0001, 0003, 0005, 0007, 0009, 0011, 0013, 0015 - 00001, 00003

        // 2nd run failures (msg list ): 03,  07
        // 2nd run failures (multicast): 003, 007 - 0003, 0007, 0011, 0015 - 00003

        // 3rd run failures (msg list ): 07
        // 3rd run failures (multicast): 007 - 0007, 0015

        Assert.Equal(["07"], failed.Item1.Select(p => p.Token));
        Assert.Equal(["007", "0007", "0015"], failed.Item2.SelectMany(p => p.Tokens));
        Assert.Equal(["Test-1", "Test-2"], failed.Item2.Select(p => p.Notification.Body));
        Assert.Equal(2, failed.Item2.Count);
    }

    [Fact]
    public async Task DispatchAsync_returns_failed_messages_on_partial_failure2()
    {
        // Arrange
        var firebaseMoq = new PartiallySucceedingFirebaseMoq(0);
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 4, 0);

        var messages = Enumerable.Range(0, 10).Select(i => new Message { Notification = new() { Body = "Test" }, Token = $"{i:00}" }).ToList();
        var multicastMessages = new List<MulticastMessage>
        {
            new()
            {
                Notification = new() { Body = "Test-1" },
                Tokens = [.. Enumerable.Range(0, 10).Select(i => $"{i:000}")],
            },
            new()
            {
                Notification = new() { Body = "Test-2" },
                Tokens = [.. Enumerable.Range(0, 17).Select(i => $"{i:0000}")],
            },
            new()
            {
                Notification = new() { Body = "Test-3" },
                Tokens = [.. Enumerable.Range(0, 5).Select(i => $"{i:00000}")],
            }
        };

        // Act
        var failed = await pushService.DispatchAsync((messages, multicastMessages));

        // Assert

        // 1st run failures (msg list ): 01,  03,  05,  07,  09
        // 1st run failures (multicast): 001, 003, 005, 007, 009 - 0001, 0003, 0005, 0007, 0009, 0011, 0013, 0015 - 00001, 00003

        // 2nd run failures (msg list ): 03,  07
        // 2nd run failures (multicast): 003, 007 - 0003, 0007, 0011, 0015 - 00003

        // 3rd run failures (msg list ): 07
        // 3rd run failures (multicast): 007 - 0007, 0015

        // 4th run failures (msg list ): []
        // 4th run failures (multicast): 0015

        Assert.Empty(failed.Item1);
        Assert.Equal(["0015"], failed.Item2.SelectMany(p => p.Tokens));
        Assert.Equal(["Test-2"], failed.Item2.Select(p => p.Notification.Body));
        Assert.Single(failed.Item2);
    }

    [Fact]
    public async Task DispatchAsync_returns_failed_messages_on_partial_failure3()
    {
        // Arrange
        var firebaseMoq = new PartiallySucceedingFirebaseMoq(0);
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 5, 0);

        var messages = Enumerable.Range(0, 10).Select(i => new Message { Notification = new() { Body = "Test" }, Token = $"{i:00}" }).ToList();
        var multicastMessages = new List<MulticastMessage>
        {
            new()
            {
                Notification = new() { Body = "Test-1" },
                Tokens = [.. Enumerable.Range(0, 10).Select(i => $"{i:000}")],
            },
            new()
            {
                Notification = new() { Body = "Test-2" },
                Tokens = [.. Enumerable.Range(0, 17).Select(i => $"{i:0000}")],
            },
            new()
            {
                Notification = new() { Body = "Test-3" },
                Tokens = [.. Enumerable.Range(0, 5).Select(i => $"{i:00000}")],
            }
        };

        // Act
        var failed = await pushService.DispatchAsync((messages, multicastMessages));

        // Assert

        // 1st run failures (msg list ): 01,  03,  05,  07,  09
        // 1st run failures (multicast): 001, 003, 005, 007, 009 - 0001, 0003, 0005, 0007, 0009, 0011, 0013, 0015 - 00001, 00003

        // 2nd run failures (msg list ): 03,  07
        // 2nd run failures (multicast): 003, 007 - 0003, 0007, 0011, 0015 - 00003

        // 3rd run failures (msg list ): 07
        // 3rd run failures (multicast): 007 - 0007, 0015

        // 4th run failures (msg list ): []
        // 4th run failures (multicast): 0015

        // 5th run failures (msg list ): []
        // 5th run failures (multicast): []

        Assert.Empty(failed.Item1);
        Assert.Empty(failed.Item2);
    }

    [Theory]
    [InlineData(ErrorCode.InvalidArgument)]
    [InlineData(ErrorCode.NotFound)]
    public async Task DispatchAsync_does_not_return_failed_messages_when_error_is_InvalidArgument_or_NotFound(ErrorCode errorCode)
    {
        // Arrange
        var firebaseMoq = new FailingNonRetryingFirebaseMoq(errorCode, default);
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 0, 0);

        var messagesSingle = new List<Message> { new() { Notification = new() { Body = "Test" }, Token = "111" } };
        var messagesMulti = new List<Message> { new() { Notification = new() { Body = "Test" }, Token = "111" }, new() { Notification = new() { Body = "Test" }, Token = "222" } };
        var multicastMessages = new List<MulticastMessage> { new() { Notification = new() { Body = "Test" }, Tokens = ["333", "444"] } };

        // Act
        var failed1 = await pushService.DispatchAsync((messagesSingle, multicastMessages));
        var failed2 = await pushService.DispatchAsync((messagesMulti, multicastMessages));

        // Assert
        Assert.Empty(failed1.Item1);
        Assert.Empty(failed1.Item2);
        Assert.Empty(failed2.Item1);
        Assert.Empty(failed2.Item2);
    }

    [Fact]
    public async Task DispatchAsync_chunks_batches_into_500_piece_parts()
    {
        // Arrange
        var firebaseMoq = new SucceedingFirebaseMoq();
        var pushService = new PushNotificationService(TimeProvider.System, firebaseMoq, Mock.Of<IPushNotificationProblemReporter>(), Mock.Of<ILogger<PushNotificationService>>(), 5, 0);

        var messages = Enumerable.Range(0, 1001).Select(i => new Message { Notification = new() { Body = $"Test-{i}" }, Token = $"{i}" }).ToList();

        // Act
        await pushService.DispatchAsync((messages, new List<MulticastMessage>()));

        // Assert
        Assert.Equal(3, firebaseMoq.SendAllAsyncCallCount);
    }
}

public abstract class FirebaseBaseMoq : IFirebaseMessagingService
{
    protected static readonly ConstructorInfo SendResponseMsgCi = typeof(SendResponse).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic,
        binder: null, [typeof(string)], modifiers: null);

    protected static readonly ConstructorInfo SendResponseErrCi = typeof(SendResponse).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic,
        binder: null, [typeof(FirebaseMessagingException)], modifiers: null);

    protected static readonly ConstructorInfo BatchResponseCi = typeof(BatchResponse).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic,
        binder: null, [typeof(IEnumerable<SendResponse>)], modifiers: null);

    protected static readonly ConstructorInfo FirebaseMessagingExceptionCi = typeof(FirebaseMessagingException).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic,
        binder: null, [typeof(ErrorCode), typeof(string), typeof(MessagingErrorCode), typeof(Exception), typeof(HttpResponseMessage)], modifiers: null);

    public int SendAsyncCallCount { get; private set; }
    public int SendAllAsyncCallCount { get; private set; }
    public int SendMulticastAsyncCallCount { get; private set; }

    protected static FirebaseMessagingException GetFirebaseMessagingException(ErrorCode errorCode, MessagingErrorCode messagingErrorCode)
        => (FirebaseMessagingException)FirebaseMessagingExceptionCi.Invoke(
        [
            errorCode,
            "error",
            messagingErrorCode,
            null,
            null
        ]);

    public virtual Task<FirebaseSendResponse> SendAsync(Message message)
    {
        SendAsyncCallCount++;
        var ex = GetFirebaseMessagingException(ErrorCode.Unknown, MessagingErrorCode.Internal);
        return Task.FromResult(new FirebaseSendResponse(ex));
    }

    public virtual Task<BatchResponse> SendAllAsync(IEnumerable<Message> messages)
    {
        SendAllAsyncCallCount++;
        return Task.FromResult((BatchResponse)null);
    }

    public virtual Task<BatchResponse> SendMulticastAsync(MulticastMessage message)
    {
        SendMulticastAsyncCallCount++;
        return Task.FromResult((BatchResponse)null);
    }

    public Task<TopicManagementResponse> SubscribeToTopicAsync(string registrationToken, string topic) => throw new NotImplementedException();

    public Task<TopicManagementResponse> UnsubscribeFromTopicAsync(string registrationToken, string topic) => throw new NotImplementedException();
}

public class SucceedingFirebaseMoq : FirebaseBaseMoq
{
    public override async Task<FirebaseSendResponse> SendAsync(Message message)
    {
        await base.SendAsync(message);
        return new FirebaseSendResponse(Guid.NewGuid().ToString());
    }

    public override async Task<BatchResponse> SendAllAsync(IEnumerable<Message> messages)
    {
        await base.SendAllAsync(messages);

        var sendResponses = messages
            .Select(_ => (SendResponse)SendResponseMsgCi.Invoke([Guid.NewGuid().ToString()]))
            .ToArray();

        return (BatchResponse)BatchResponseCi.Invoke([sendResponses]);
    }

    public override async Task<BatchResponse> SendMulticastAsync(MulticastMessage message)
    {
        await base.SendMulticastAsync(message);

        var sendResponses = message.Tokens
            .Select(_ => (SendResponse)SendResponseMsgCi.Invoke([Guid.NewGuid().ToString()]))
            .ToArray();

        return (BatchResponse)BatchResponseCi.Invoke([sendResponses]);
    }
}

public class FailingFirebaseMoq : FirebaseBaseMoq
{
    public override Task<FirebaseSendResponse> SendAsync(Message message) => base.SendAsync(message);

    public override async Task<BatchResponse> SendAllAsync(IEnumerable<Message> messages)
    {
        await base.SendAllAsync(messages);
        var ex = GetFirebaseMessagingException(ErrorCode.Unknown, MessagingErrorCode.Internal);

        var sendResponses = messages
            .Select(_ => (SendResponse)SendResponseErrCi.Invoke([ex]))
            .ToArray();

        return (BatchResponse)BatchResponseCi.Invoke([sendResponses]);
    }

    public override async Task<BatchResponse> SendMulticastAsync(MulticastMessage message)
    {
        await base.SendMulticastAsync(message);
        var ex = GetFirebaseMessagingException(ErrorCode.Unknown, MessagingErrorCode.Internal);

        var sendResponses = message.Tokens
            .Select(_ => (SendResponse)SendResponseErrCi.Invoke([ex]))
            .ToArray();

        return (BatchResponse)BatchResponseCi.Invoke([sendResponses]);
    }
}

public class FailingNonRetryingFirebaseMoq : FirebaseBaseMoq
{
    private readonly ErrorCode _errorCode;
    private readonly MessagingErrorCode _messagingErrorCode;

    public FailingNonRetryingFirebaseMoq(ErrorCode errorCode, MessagingErrorCode messagingErrorCode)
    {
        _errorCode = errorCode;
        _messagingErrorCode = messagingErrorCode;
    }

    public override async Task<FirebaseSendResponse> SendAsync(Message message)
    {
        await base.SendAsync(message);
        var ex = GetFirebaseMessagingException(_errorCode, _messagingErrorCode);
        return new FirebaseSendResponse(ex);
    }

    public override async Task<BatchResponse> SendAllAsync(IEnumerable<Message> messages)
    {
        await base.SendAllAsync(messages);
        var ex = GetFirebaseMessagingException(_errorCode, _messagingErrorCode);

        var sendResponses = messages
            .Select(_ => (SendResponse)SendResponseErrCi.Invoke([ex]))
            .ToArray();

        return (BatchResponse)BatchResponseCi.Invoke([sendResponses]);
    }

    public override async Task<BatchResponse> SendMulticastAsync(MulticastMessage message)
    {
        await base.SendMulticastAsync(message);
        var ex = GetFirebaseMessagingException(_errorCode, _messagingErrorCode);

        var sendResponses = message.Tokens
            .Select(_ => (SendResponse)SendResponseErrCi.Invoke([ex]))
            .ToArray();

        return (BatchResponse)BatchResponseCi.Invoke([sendResponses]);
    }
}

public class PartiallySucceedingFirebaseMoq : FirebaseBaseMoq
{
    private readonly int _failAttemptCount;

    public PartiallySucceedingFirebaseMoq(int failAttemptCount) => _failAttemptCount = failAttemptCount;

    public override async Task<FirebaseSendResponse> SendAsync(Message message)
    {
        var errResponse = await base.SendAsync(message);
        return SendAsyncCallCount > _failAttemptCount
            ? new FirebaseSendResponse(Guid.NewGuid().ToString())
            : errResponse;
    }

    public override async Task<BatchResponse> SendAllAsync(IEnumerable<Message> messages)
    {
        await base.SendAllAsync(messages);
        var ex = GetFirebaseMessagingException(ErrorCode.Unknown, MessagingErrorCode.Internal);

        var sendResponses = messages
            .Select((_, i) => i % 2 == 0
                ? (SendResponse)SendResponseMsgCi.Invoke([Guid.NewGuid().ToString()])
                : (SendResponse)SendResponseErrCi.Invoke([ex]))
            .ToArray();

        return (BatchResponse)BatchResponseCi.Invoke([sendResponses]);
    }

    public override async Task<BatchResponse> SendMulticastAsync(MulticastMessage message)
    {
        await base.SendMulticastAsync(message);
        var ex = GetFirebaseMessagingException(ErrorCode.Unknown, MessagingErrorCode.Internal);

        var sendResponses = message.Tokens
            .Select((_, i) => i % 2 == 0
                ? (SendResponse)SendResponseMsgCi.Invoke([Guid.NewGuid().ToString()])
                : (SendResponse)SendResponseErrCi.Invoke([ex]))
            .ToArray();

        return (BatchResponse)BatchResponseCi.Invoke([sendResponses]);
    }
}
