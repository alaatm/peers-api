using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Mashkoor.Core.AzureServices;
using Mashkoor.Core.AzureServices.AppInsights;

namespace Mashkoor.Core.Test.AzureServices;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureAppInsights_adds_all_required_services()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var serviceProvider = serviceCollection
            .AddLogging()
            .AddAzureAppInsights()
            .BuildServiceProvider();

        // Assert
        Assert.IsType<TelemetryEnrichment>(serviceProvider.GetRequiredService<ITelemetryInitializer>());
    }
}
