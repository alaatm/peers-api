using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Peers.Core.Background.Jobs;

namespace Peers.Core.Test.Background.Jobs;

public class BackgroundJobTests
{
    [Fact]
    public void Ctor_Creates_Logger()
    {
        // Arrange
        var loggerFactoryMoq = new Mock<ILoggerFactory>(MockBehavior.Strict);
        loggerFactoryMoq.Setup(m => m.CreateLogger(typeof(TestJob).FullName.Replace('+', '.'))).Returns(Mock.Of<ILogger>());

        // Act
        _ = new BackgroundJob<TestJob>(TimeProvider.System, Mock.Of<IServiceProvider>(), loggerFactoryMoq.Object);

        // Assert
        loggerFactoryMoq.VerifyAll();
    }

    [Fact]
    public async Task Cancelled_BeforeLoopStarts_NoRun()
    {
        var start = DateTimeOffset.Parse("2025-09-13T00:00:00Z");
        var time = new FakeTimeProvider(start);
        var bj = CreateBackgroundJob<TestJob>(time, out var counter, out var collector);

        using var cts = new CancellationTokenSource();

        await bj.StartAsync(cts.Token);
        cts.Cancel(); // Cancel before starting

        // Wait for the background task to complete (should be immediate)
        try
        {
            await bj.ExecuteTask.WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch (TaskCanceledException)
        {
            // Expected
        }

        Assert.Equal(0, counter.Count);
        Assert.Empty(collector.SeenIds);
    }

    [Fact]
    public async Task Cancelled_DuringDelay_NoRun()
    {
        var start = DateTimeOffset.Parse("2025-09-13T00:00:00Z");
        var time = new FakeTimeProvider(start);
        var bj = CreateBackgroundJob<TestJob>(time, out var counter, out var collector);

        using var cts = new CancellationTokenSource();
        await bj.StartAsync(cts.Token);

        // Let the job start and schedule its delay
        await WaitForDelayReachedAsync(100);

        // Advance time by just under 1 second, so delay is not yet complete
        time.Advance(TimeSpan.FromMilliseconds(900));

        // Cancel before the job can run
        cts.Cancel();

        // Advance the remaining time to complete the delay (should not run)
        time.Advance(TimeSpan.FromMilliseconds(100));

        await bj.ExecuteTask.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(0, counter.Count);
        Assert.Empty(collector.SeenIds);
    }

    [Fact]
    public async Task Cancelled_AfterDelay_BeforeRun_NoRun()
    {
        var start = DateTimeOffset.Parse("2025-09-13T00:00:00Z");
        var time = new FakeTimeProvider(start);
        var bj = CreateBackgroundJob<TestJob>(time, out var counter, out var collector);

        using var cts = new CancellationTokenSource();
        await bj.StartAsync(cts.Token);

        // Let the job start and schedule its delay
        await WaitForDelayReachedAsync(100);

        // Advance time by just under 1 second, so delay is not yet complete
        time.Advance(TimeSpan.FromMilliseconds(900));

        // Cancel before the job can run
        cts.Cancel();

        // Advance the remaining time to complete the delay (should not run)
        time.Advance(TimeSpan.FromMilliseconds(100));

        await bj.ExecuteTask.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(0, counter.Count);
        Assert.Empty(collector.SeenIds);
    }

    [Fact]
    public async Task NoRun_WhenNo_NextOccurrence()
    {
        // Arrange
        var bj = CreateBackgroundJob<ImpossibleJob>(TimeProvider.System, out _, out _);

        // Act
        await bj.StartAsync(default);
        await bj.ExecuteTask;

        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, bj.ExecuteTask.Status);
    }

    [Fact]
    public async Task OneRun_Executes_And_UsesNewScope()
    {
        var start = DateTimeOffset.Parse("2025-09-13T00:00:00Z");
        var time = new FakeTimeProvider(start);
        var bj = CreateBackgroundJob<TestJob>(time, out var counter, out var collector);

        await bj.StartAsync(default);

        await WaitForDelayReachedAsync(100);
        time.Advance(TimeSpan.FromSeconds(1));

        Assert.True(SpinWait.SpinUntil(() => counter.Count == 1, 40_000));
        Assert.True(SpinWait.SpinUntil(() => collector.SeenIds.Count == 1, 40_000));
    }

    [Fact]
    public async Task MultipleRuns_UseDistinctScopes()
    {
        var start = DateTimeOffset.Parse("2025-09-13T00:00:00Z");
        var time = new FakeTimeProvider(start);
        var bj = CreateBackgroundJob<TestJob>(time, out var counter, out var collector);

        await bj.StartAsync(default);

        // Run #1
        await WaitForDelayReachedAsync(100);
        time.Advance(TimeSpan.FromSeconds(1));

        // Run #2
        await WaitForDelayReachedAsync(100);
        time.Advance(TimeSpan.FromSeconds(1));

        // Run #3
        await WaitForDelayReachedAsync(100);
        time.Advance(TimeSpan.FromSeconds(1));

        Assert.True(SpinWait.SpinUntil(() => counter.Count == 3, 40_000));
        Assert.True(SpinWait.SpinUntil(() => collector.SeenIds.Distinct().Count() == 3, 40_000));
    }

    [Fact]
    public async Task NoCrash_When_JobThrows()
    {
        // Arrange
        var bj = CreateBackgroundJob<CrashingJob>(TimeProvider.System, out var counter, out _);

        // Act
        await bj.StartAsync(default);

        // Assert
        Assert.True(SpinWait.SpinUntil(() => counter.Count >= 1, 40_000));
    }

    private static BackgroundJob<T> CreateBackgroundJob<T>(
        TimeProvider timeProvider,
        out RunCounter counter,
        out RunIdCollector collector)
        where T : IJob, new()
    {
        counter = new RunCounter();
        collector = new RunIdCollector();
        return new(timeProvider, BuildRootProvider(counter, collector), LoggerFactory.Create(_ => { }));
    }

    private static ServiceProvider BuildRootProvider(RunCounter counter, RunIdCollector collector) => new ServiceCollection()
        .AddScoped(_ => new ScopedRunId())   // unique per job run
        .AddSingleton(counter)               // shared counter instance
        .AddSingleton(collector)             // shared collector instance
        .BuildServiceProvider();

    private static async Task WaitForDelayReachedAsync(int delayMs)
    {
        for (var i = 0; i < delayMs; i++)
        {
            await Task.Yield();
            await Task.Delay(1);
        }
    }

    private class RunCounter
    {
        private int _count;
        public int Count => _count;
        public void Increment() => Interlocked.Increment(ref _count);
    }

    private class ScopedRunId
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    private class RunIdCollector
    {
        public ConcurrentBag<Guid> SeenIds { get; } = [];
    }

    private class TestJob : IJob
    {
        public string Name => "TestJob";
        public string CronExpression => "@every_second";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken)
        {
            // Resolve per-run scoped services
            var counter = services.GetRequiredService<RunCounter>();
            var runId = services.GetRequiredService<ScopedRunId>();
            var collector = services.GetRequiredService<RunIdCollector>();

            counter.Increment();
            collector.SeenIds.Add(runId.Id);
            return Task.CompletedTask;
        }
    }

    private class CrashingJob : IJob
    {
        public string Name => "CrashingJob";
        public string CronExpression => "@every_second";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken)
        {
            // Resolve per-run scoped services
            var counter = services.GetRequiredService<RunCounter>();
            var runId = services.GetRequiredService<ScopedRunId>();
            var collector = services.GetRequiredService<RunIdCollector>();

            counter.Increment();
            collector.SeenIds.Add(runId.Id);
            throw new InvalidOperationException();
        }
    }

    private class ImpossibleJob : IJob
    {
        public string Name => "ImpossibleJob";
        public string CronExpression => "0 0 31 2 *";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("Asia/Riyadh");

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken) => Task.CompletedTask;
    }
}
