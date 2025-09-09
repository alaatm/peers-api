using Peers.Core.Domain;

namespace Peers.Core.Background;

/// <summary>
/// Represents a publisher contract.
/// </summary>
public interface IProducer
{
    /// <summary>
    /// Publishes the specified message.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    ValueTask PublishAsync(ITraceableNotification message, CancellationToken cancellationToken = default);
}
