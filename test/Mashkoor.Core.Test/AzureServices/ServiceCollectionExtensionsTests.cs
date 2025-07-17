using Mashkoor.Core.AzureServices;
using Mashkoor.Core.AzureServices.AppInsights;
using Mashkoor.Core.AzureServices.Configuration;
using Mashkoor.Core.AzureServices.Storage;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mashkoor.Core.Test.AzureServices;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureStorage_adds_required_services()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "azure:storageConnectionString", "UseDevelopmentStorage=true" },
            })
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddAzureStorage(config)
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IStorageManager>();
        serviceProvider.GetRequiredService<IValidateOptions<AzureConfig>>();
        serviceProvider.GetRequiredService<AzureConfig>();
    }

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
