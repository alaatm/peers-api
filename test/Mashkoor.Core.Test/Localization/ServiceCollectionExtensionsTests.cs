#if DEBUG
using Microsoft.AspNetCore.Http;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Mashkoor.Core.Localization;

namespace Mashkoor.Core.Test.Localization;

public class ServiceCollectionExtensionsTests
{
#if DEBUG
    [Fact]
    public void AddLocalizationWithTracking_adds_requires_services()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var serviceProvider = services
            .AddLogging()
            .AddLocalizationWithTracking()
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IMissingKeyTrackerService>();
        serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        serviceProvider.GetRequiredService<IStringLocalizer<ServiceCollectionExtensionsTests>>();
        serviceProvider.GetRequiredService<IHttpContextAccessor>();
    }
#else
    [Fact]
    public void AddLocalizationWithTracking_adds_requires_services()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var serviceProvider = services
            .AddLogging()
            .AddLocalizationWithTracking()
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        serviceProvider.GetRequiredService<IStringLocalizer<ServiceCollectionExtensionsTests>>();
    }
#endif
}
