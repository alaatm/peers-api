using Peers.Core.RateLimiting;

namespace Peers.Core.Test.RateLimiting;

public class RateLimitingOptionsTests
{
    [Fact]
    public void TokenBucketLimitingOptions_Options_maps_to_TokenBucketRateLimiterOptions()
    {
        // Arrange
        var options = new TokenBucketLimitingOptions()
        {
            QueueLimit = 1,
            TokenLimit = 2,
            TokensPerPeriod = 3,
            AutoReplenishment = true,
            ReplenishmentPeriod = 4,
        };

        // Act
        var result = options.Options;

        // Assert
        Assert.Equal(options.QueueLimit, result.QueueLimit);
    }

    [Fact]
    public void ConcurrencyLimitingOptions_Options_maps_to_ConcurrencyLimiterOptions()
    {
        // Arrange
        var options = new ConcurrencyLimitingOptions()
        {
            QueueLimit = 1,
            PermitLimit = 2,
        };

        // Act
        var result = options.Options;

        // Assert
        Assert.Equal(options.QueueLimit, result.QueueLimit);
    }
}
