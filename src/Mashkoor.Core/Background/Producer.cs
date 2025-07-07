using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Mashkoor.Core.Domain;

namespace Mashkoor.Core.Background;

/// <summary>
/// Represents the main publisher of the application.
/// This should be configured as a singleton.
/// </summary>
public sealed class Producer : IProducer
{
    private readonly ChannelWriter<ITraceableNotification> _writer;
    private readonly ILogger<Producer> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="Producer"/>.
    /// </summary>
    /// <param name="writer">The channel writer.</param>
    /// <param name="logger">The instance logger.</param>
    public Producer(ChannelWriter<ITraceableNotification> writer, ILogger<Producer> logger)
    {
        _writer = writer;
        _logger = logger;
    }

    /// <summary>
    /// Publishes the specified message.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async ValueTask PublishAsync(
        [NotNull] ITraceableNotification message,
        CancellationToken cancellationToken = default)
    {
        using var loggingScope = _logger.BeginScope(message.TraceIdentifier ?? "unknown");
        _logger.PublishingMessage(message.GetType());
        await _writer.WriteAsync(message, cancellationToken);
    }
}
