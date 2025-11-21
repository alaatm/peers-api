using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peers.Core.GoogleServices;
using Peers.Core.GoogleServices.Configuration;
using Peers.Core.GoogleServices.Maps;

namespace Peers.Core.Test.GoogleServices;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGoogleServices_adds_google_services()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "google:apiKey", "123" },
            })
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddGoogleServices(config)
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IGoogleMapsService>();
        serviceProvider.GetRequiredService<GoogleConfig>();
    }
}
