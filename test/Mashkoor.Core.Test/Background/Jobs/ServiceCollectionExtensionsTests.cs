using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mashkoor.Core.Background.Jobs;

namespace Mashkoor.Core.Test.Background.Jobs;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBackgroundJob_adds_background_job_as_hosted_service()
    {
        // Arrange
        var services = new ServiceCollection().AddLogging();

        // Act
        services.AddBackgroundJob<TestJob>();

        // Assert
        var sp = services.BuildServiceProvider();
        Assert.IsType<BackgroundJob<TestJob>>(sp.GetRequiredService<IHostedService>());
    }

    private class TestJob : IJob
    {
        public string Name => "Test Job";
        public string CronExpression => "* * * * *";
        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");

        public Task RunAsync(IServiceProvider services, ILogger log, CancellationToken stoppingToken) => Task.CompletedTask;
    }
}
