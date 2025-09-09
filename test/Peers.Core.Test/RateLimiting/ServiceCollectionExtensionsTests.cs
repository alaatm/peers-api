using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Peers.Core.RateLimiting;

namespace Peers.Core.Test.RateLimiting;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRateLimiting_adds_required_services()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "rateLimiting:perUserRateLimit:queueLimit", "0" },
                { "rateLimiting:perUserRateLimit:tokenLimit", "300" },
                { "rateLimiting:perUserRateLimit:tokensPerPeriod", "300" },
                { "rateLimiting:perUserRateLimit:autoReplenishment", "true" },
                { "rateLimiting:perUserRateLimit:replenishmentPeriod", "60" },
                { "rateLimiting:anonRateLimit:queueLimit", "0" },
                { "rateLimiting:anonRateLimit:tokenLimit", "200" },
                { "rateLimiting:anonRateLimit:tokensPerPeriod", "200" },
                { "rateLimiting:anonRateLimit:autoReplenishment", "true" },
                { "rateLimiting:anonRateLimit:replenishmentPeriod", "60" },
                { "rateLimiting:anonConcurrencyLimit:queueLimit", "0" },
                { "rateLimiting:anonConcurrencyLimit:permitLimit", "10" },
            })
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddRateLimiting(config)
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<IOptions<RateLimiterOptions>>();
    }
}
