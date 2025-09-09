namespace Peers.Core.Background.Jobs;

/// <summary>
/// Represents a job.
/// </summary>
public interface IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    /// <value></value>
    string Name { get; }
    /// <summary>
    /// The cron expression of the job.
    /// </summary>
    /// <value></value>
    string CronExpression { get; }
    /// <summary>
    /// The time zone to use when calculating the next occurrence.
    /// </summary>
    /// <value></value>
    TimeZoneInfo TimeZoneInfo { get; }

    /// <summary>
    /// Runs the job.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <param name="log">The logger instance.</param>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns></returns>
    Task RunAsync(
        IServiceProvider services,
        ILogger log,
        CancellationToken stoppingToken);
}
