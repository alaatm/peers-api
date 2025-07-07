using System.Diagnostics;
using Cronos;

namespace Mashkoor.Core.Background.Jobs;

/// <summary>
/// Represents a background job.
/// </summary>
/// <typeparam name="T">The type of the job.</typeparam>
public sealed class BackgroundJob<T> : BackgroundService
    where T : IJob
{
    private readonly T _job;
    private readonly CronExpression _cronExpression;
    private readonly TimeProvider _timeProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<T> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundJob{T}"/> class.
    /// </summary>
    /// <param name="timeProvider">The time provider.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public BackgroundJob(
        TimeProvider timeProvider,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
    {
        _job = Activator.CreateInstance<T>();
        ArgumentException.ThrowIfNullOrEmpty(_job.Name);
        ArgumentNullException.ThrowIfNull(_job.CronExpression);
        ArgumentNullException.ThrowIfNull(_job.TimeZoneInfo);

        _cronExpression = CronExpression.Parse(_job.CronExpression);
        _timeProvider = timeProvider;
        _serviceProvider = serviceProvider;
        _logger = loggerFactory.CreateLogger<T>();
    }

    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var loggerScope = _logger.BeginScope(_job.Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = _timeProvider.UtcNow();
            var next = _cronExpression.GetNextOccurrence(now, _job.TimeZoneInfo);

            if (next.HasValue)
            {
                var delay = next.Value - now;

                _logger.JobScheduled(_job.Name, delay, next.Value);

                await Task.Delay(delay, _timeProvider, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    _logger.JobStarted(_job.Name);
                    var startTimestamp = Stopwatch.GetTimestamp();
                    await RunJobAsync(stoppingToken);
                    _logger.JobFinished(_job.Name, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.JobCaughtException(ex, _job.Name);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
            else
            {
                break;
            }
        }

        _logger.JobNotRun(_job.Name);
    }

    private async Task RunJobAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        await _job.RunAsync(scope.ServiceProvider, _logger, stoppingToken);
    }
}
