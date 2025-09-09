using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Peers.Core.Background.Jobs;

namespace Peers.Core.Test.Background.Jobs;

public class BackgroundJobTests
{
    [Fact]
    public void Ctor_creates_logger()
    {
        // Arrange
        var loggerFactoryMoq = new Mock<ILoggerFactory>(MockBehavior.Strict);
        loggerFactoryMoq.Setup(m => m.CreateLogger("Peers.Core.Test.Background.Jobs.BackgroundJobTests.TestJob")).Returns(Mock.Of<ILogger>());

        // Act
        _ = new BackgroundJob<TestJob>(TimeProvider.System, Mock.Of<IServiceProvider>(), loggerFactoryMoq.Object);

        // Assert
        loggerFactoryMoq.VerifyAll();
    }

    [Fact]
    public async Task Exists_job_when_cancellation_is_requested_fromLoop()
    {
        // Arrange
        var loggerFactoryMoq = new Mock<ILoggerFactory>();
        loggerFactoryMoq.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        var backgroundJob = new BackgroundJob<TestJob>(TimeProvider.System, new ServiceCollection().BuildServiceProvider(), loggerFactoryMoq.Object);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await backgroundJob.StartAsync(cts.Token);

        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, backgroundJob.ExecuteTask.Status);
    }

    [Fact]
    public async Task Exists_job_when_cancellation_is_requested()
    {
        // Arrange
        var loggerFactoryMoq = new Mock<ILoggerFactory>();
        loggerFactoryMoq.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        var backgroundJob = new BackgroundJob<SlowJob>(TimeProvider.System, new ServiceCollection().BuildServiceProvider(), loggerFactoryMoq.Object);

        var cts = new CancellationTokenSource();

        // Act
        await backgroundJob.StartAsync(cts.Token);
        cts.Cancel();
        await backgroundJob.ExecuteTask;

        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, backgroundJob.ExecuteTask.Status);
    }

    [Fact]
    public async Task Does_not_run_when_there_is_no_next_occurrence()
    {
        // Arrange
        var loggerFactoryMoq = new Mock<ILoggerFactory>();
        loggerFactoryMoq.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        var backgroundJob = new BackgroundJob<ImpossibleJob>(TimeProvider.System, new ServiceCollection().BuildServiceProvider(), loggerFactoryMoq.Object);

        // Act
        await backgroundJob.StartAsync(default);

        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, backgroundJob.ExecuteTask.Status);
    }

    [Fact]
    public async Task Executes_job()
    {
        // Arrange
        var loggerFactoryMoq = new Mock<ILoggerFactory>();
        loggerFactoryMoq.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        var backgroundJob = new BackgroundJob<TestJob>(TimeProvider.System, new ServiceCollection().BuildServiceProvider(), loggerFactoryMoq.Object);

        // Act
        await backgroundJob.StartAsync(default);

        // Assert
        var job = GetJobRef(backgroundJob);
        Assert.True(SpinWait.SpinUntil(() => job.DidRun, 40_000));
    }

    [Fact]
    public async Task Doesnt_crash_when_job_throws()
    {
        // Arrange
        var loggerFactoryMoq = new Mock<ILoggerFactory>();
        loggerFactoryMoq.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        var backgroundJob = new BackgroundJob<TestCrashingJob>(TimeProvider.System, new ServiceCollection().BuildServiceProvider(), loggerFactoryMoq.Object);

        // Act
        await backgroundJob.StartAsync(default);

        // Assert
        var job = GetJobRef(backgroundJob);
        Assert.True(SpinWait.SpinUntil(() => job.DidRun, 40_000));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await job.RunAsync(null, null, default));
    }

    [Fact]
    public async Task Passes_a_serviceProvider_scope_to_job()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddScoped<IScopedService, ScopedService>()
            .BuildServiceProvider();

        var loggerFactoryMoq = new Mock<ILoggerFactory>();
        loggerFactoryMoq.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        var backgroundJob = new BackgroundJob<TestJobScopeTest>(TimeProvider.System, services, loggerFactoryMoq.Object);

        // Act
        await backgroundJob.StartAsync(default);

        // Assert
        var job = GetJobRef(backgroundJob);
        Assert.True(SpinWait.SpinUntil(() => job.DidRun, 40_000));
        Assert.True(SpinWait.SpinUntil(() => job.ScopedServiceDisposed, 40_000));
    }

    private static T GetJobRef<T>(BackgroundJob<T> backgroundJob) where T : class, IJob => typeof(BackgroundJob<T>)
        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .Single(p => p.Name == "_job")
        .GetValue(backgroundJob) as T;

    private class TestJob : IJob
    {
        public bool DidRun { get; set; }
        public string Name => "TestJob";
        public string CronExpression => "@every_second";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken)
        {
            DidRun = true;
            return Task.CompletedTask;
        }
    }

    private class SlowJob : IJob
    {
        public string Name => "SlowJob";
        public string CronExpression => "@daily";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken) => Task.CompletedTask;
    }

    private class ImpossibleJob : IJob
    {
        public string Name => "ImpossibleJob";
        public string CronExpression => "0 0 31 2 *";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken) => Task.CompletedTask;
    }

    private class TestCrashingJob : IJob
    {
        public bool DidRun { get; set; }
        public string Name => "TestCrashingJob";
        public string CronExpression => "@every_second";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken)
        {
            DidRun = true;
            throw new InvalidOperationException();
        }
    }

    private class TestJobScopeTest : IJob
    {
        public bool DidRun { get; set; }
        public bool ScopedServiceDisposed { get; set; }
        public string Name => "TestJobScopeTest";
        public string CronExpression => "@every_second";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken)
        {
            var scopedService = services.GetRequiredService<IScopedService>();
            scopedService.Tag(this);
            DidRun = true;
            return Task.CompletedTask;
        }
    }

    private interface IScopedService
    {
        void Tag(TestJobScopeTest job);
    }

    private class ScopedService : IScopedService, IDisposable
    {
        private TestJobScopeTest _job;
        public void Tag(TestJobScopeTest job) => _job = job;
        public void Dispose() => _job.ScopedServiceDisposed = true;
    }
}
