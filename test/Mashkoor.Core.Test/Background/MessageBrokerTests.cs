using System.Diagnostics;
using System.Threading.Channels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mashkoor.Core.Background;
using Mashkoor.Core.Domain;
using Mashkoor.Core.Identity;

namespace Mashkoor.Core.Test.Background;

public class MessageBrokerTests
{
    [Fact]
    public async Task ExecuteAsync_starts_all_consumers()
    {
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

        // Arrange
        var services = new ServiceCollection()
            .AddIdentityInfo()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<TestMessage>())
            .BuildServiceProvider();

        var count = 1000;
        var channel = Channel.CreateUnbounded<ITraceableNotification>();
        var consumers = Enumerable.Range(1, count)
            .Select(i => new Consumer(channel.Reader, services, Mock.Of<ILogger<Consumer>>(), i))
            .ToArray();
        var broker = new MessageBroker(consumers);

        var messages = Enumerable.Range(0, count)
            .Select(i => new TestMessage())
            .ToArray();

        foreach (var message in messages)
        {
            await channel.Writer.WriteAsync(message);
        }

        // Act
        _ = broker.InternalExecuteAsync();
        await Task.Delay(350); // Wait until all messaged are handled concurrently

        // Assert
        var handledMessages = messages.Where(p => p.HandledOn.HasValue).ToArray();
        Assert.Equal(count, handledMessages.Length);
    }

    private class TestMessage : ITraceableNotification
    {
        public string TraceIdentifier { get; set; }
        public DateTime? HandledOn { get; set; }
    }
    private class TestMessageHandler : INotificationHandler<TestMessage>
    {
        public async Task Handle(TestMessage notification, CancellationToken cancellationToken)
        {
            notification.HandledOn = DateTime.UtcNow;
            await Task.Delay(200, cancellationToken);
        }
    }
}
