using MediatR;
using System.Threading.Channels;
using Peers.Core.Domain;
using System.Diagnostics;

namespace Peers.Core.Background;

/// <summary>
/// Represents a subscriber.
/// </summary>
public sealed class Consumer : IConsumer
{
    private readonly ChannelReader<ITraceableNotification> _reader;
    private readonly IServiceProvider _services;
    private readonly ILogger<Consumer> _logger;
    private readonly int _instanceId;

    /// <summary>
    /// Creates a new instance of <see cref="Consumer"/>.
    /// </summary>
    /// <param name="reader">The channel reader.</param>
    /// <param name="service">The service provider.</param>
    /// <param name="logger">The instance logger.</param>
    /// <param name="instanceId">The instance id.</param>
    public Consumer(
        ChannelReader<ITraceableNotification> reader,
        IServiceProvider service,
        ILogger<Consumer> logger,
        int instanceId)
    {
        _reader = reader;
        _services = service;
        _logger = logger;
        _instanceId = instanceId;
    }

    /// <summary>
    /// Starts consuming incoming messages indefinitely or until shutdown.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task BeginConsumeAsync(CancellationToken cancellationToken = default)
    {
        _logger.ConsumerStarting(_instanceId);

        try
        {
            await foreach (var message in _reader.ReadAllAsync(cancellationToken))
            {
                var messageType = message.GetType();
                using var loggingScope = _logger.BeginScope(message.TraceIdentifier ?? "unknown");

                using var scope = _services.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    _logger.ConsumerReceivedMessage(_instanceId, messageType);
                    var startTimestamp = Stopwatch.GetTimestamp();
                    await mediator.Publish(message, cancellationToken);
                    _logger.ConsumerExecutedMessage(_instanceId, messageType, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.ConsumerCaughtException(ex, _instanceId);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }
        catch (OperationCanceledException)
        {
            _logger.ConsumerForcedStop(_instanceId);
        }

        _logger.ConsumerShuttingDown(_instanceId);
    }
}
