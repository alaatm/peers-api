namespace Peers.Core.Background;

/// <summary>
/// Represents a background message broker.
/// </summary>
public sealed class MessageBroker : BackgroundService
{
    private readonly IEnumerable<IConsumer> _consumers;

    /// <summary>
    /// Creates a new instance of <see cref="MessageBroker"/>.
    /// </summary>
    /// <param name="consumers">The list of registered consumers.</param>
    public MessageBroker(IEnumerable<IConsumer> consumers)
        => _consumers = consumers;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Begin consuming on all registered consumers.
        var consumerTasks = _consumers.Select(c => c.BeginConsumeAsync(stoppingToken));
        return Task.WhenAll(consumerTasks);
    }

    // For tests access only
    internal Task InternalExecuteAsync(CancellationToken stoppingToken = default)
        => ExecuteAsync(stoppingToken);
}
