using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Peers.Core.Background;
using Peers.Core.Domain;

namespace Peers.Core.Test.Background;

public class ProducerTests
{
    [Fact]
    public async Task PublishAsync_writes_message_to_channel()
    {
        // Arrange
        var message = new TestMessage();
        var channel = Channel.CreateBounded<ITraceableNotification>(1);
        var producer = new Producer(channel.Writer, Mock.Of<ILogger<Producer>>());

        // Act
        await producer.PublishAsync(message);

        // Assert
        Assert.True(channel.Reader.TryRead(out var readMessage));
        Assert.Same(message, readMessage);
    }

    private record TestMessage() : ITraceableNotification
    {
        public string TraceIdentifier { get; set; }
    }
}
