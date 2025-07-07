namespace Mashkoor.Core.Background;

/// <summary>
/// Represents a subscriber contract.
/// </summary>
public interface IConsumer
{
    /// <summary>
    /// Starts consuming incoming messages indefinitely or until shutdown.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task BeginConsumeAsync(CancellationToken cancellationToken = default);
}
