using System.Threading.Channels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Peers.Core.Background;
using Peers.Core.Domain;
using Peers.Core.Identity;

namespace Peers.Core.Test.Background;

public class ConsumerTests
{
    private const int SpinTimeout = 10_000;

    [Fact]
    public async Task BeginConsumeAsync_reads_messages()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddIdentityInfo()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<TestMessage>())
            .BuildServiceProvider();

        var channel = Channel.CreateBounded<ITraceableNotification>(10);
        var consumer = new Consumer(channel.Reader, services, Mock.Of<ILogger<Consumer>>(), 1);

        var messages = Enumerable.Range(0, 10)
            .Select(i => new TestMessage())
            .ToArray();

        foreach (var message in messages)
        {
            await channel.Writer.WriteAsync(message);
        }

        // Act
        _ = consumer.BeginConsumeAsync();

        // Assert
        SpinWait.SpinUntil(() => messages.All(p => p.HandledOn.HasValue), SpinTimeout);
        var handledMessages = messages.Where(p => p.HandledOn.HasValue).ToArray();
        Assert.Equal(10, handledMessages.Length);
    }

    [Fact]
    public async Task BeginConsumeAsync_doesnt_crash_when_a_handler_throws()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddIdentityInfo()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<TestMessage>())
            .BuildServiceProvider();

        var channel = Channel.CreateBounded<ITraceableNotification>(1);
        var consumer = new Consumer(channel.Reader, services, Mock.Of<ILogger<Consumer>>(), 1);

        var message = new ThrowingTestMessage();
        await channel.Writer.WriteAsync(message);

        // Act
        _ = consumer.BeginConsumeAsync();

        // Assert
        SpinWait.SpinUntil(() => message.HandledOn.HasValue, SpinTimeout);
        Assert.NotNull(message.HandledOn);

        // We didnt crash. write another message to test we can still consume
        message = new ThrowingTestMessage();
        await channel.Writer.WriteAsync(message);
        SpinWait.SpinUntil(() => message.HandledOn.HasValue, SpinTimeout);
        Assert.NotNull(message.HandledOn);
    }

    [Fact]
    public void BeginConsumeAsync_exits_when_cancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var channel = Channel.CreateBounded<ITraceableNotification>(1);
        var consumer = new Consumer(channel.Reader, Mock.Of<IServiceProvider>(), Mock.Of<ILogger<Consumer>>(), 1);

        // Act
        var t = consumer.BeginConsumeAsync(cts.Token);
        SpinWait.SpinUntil(() => t.Status == TaskStatus.Running, SpinTimeout);
        cts.Cancel();

        // Assert
        SpinWait.SpinUntil(() => t.Status == TaskStatus.RanToCompletion, SpinTimeout);
        Assert.Equal(TaskStatus.RanToCompletion, t.Status);
    }

    private class TestMessage : ITraceableNotification
    {
        public string TraceIdentifier { get; set; }
        public DateTime? HandledOn { get; set; }
    }
    private class TestMessageHandler : INotificationHandler<TestMessage>
    {
        public Task Handle(TestMessage notification, CancellationToken cancellationToken)
        {
            notification.HandledOn = DateTime.UtcNow;
            return Task.CompletedTask;
        }
    }

    private class ThrowingTestMessage : ITraceableNotification
    {
        public string TraceIdentifier { get; set; }
        public DateTime? HandledOn { get; set; }
    }
    private class ThrowingTestMessageHandler : INotificationHandler<ThrowingTestMessage>
    {
        public Task Handle(ThrowingTestMessage notification, CancellationToken cancellationToken)
        {
            notification.HandledOn = DateTime.UtcNow;
            throw new InvalidOperationException("test");
        }
    }
}
