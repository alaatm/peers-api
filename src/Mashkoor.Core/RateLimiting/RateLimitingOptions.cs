using System.Threading.RateLimiting;

namespace Mashkoor.Core.RateLimiting;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public TokenBucketLimitingOptions PerUserRateLimit { get; set; } = default!;
    public TokenBucketLimitingOptions AnonRateLimit { get; set; } = default!;
    public ConcurrencyLimitingOptions AnonConcurrencyLimit { get; set; } = default!;
}

public class TokenBucketLimitingOptions
{
    public int QueueLimit { get; set; }
    public int TokenLimit { get; set; }
    public int TokensPerPeriod { get; set; }
    public bool AutoReplenishment { get; set; }
    public int ReplenishmentPeriod { get; set; }

    public TokenBucketRateLimiterOptions Options => new()
    {
        QueueLimit = QueueLimit,
        TokenLimit = TokenLimit,
        TokensPerPeriod = TokensPerPeriod,
        AutoReplenishment = AutoReplenishment,
        ReplenishmentPeriod = TimeSpan.FromSeconds(ReplenishmentPeriod),
    };
}

public class ConcurrencyLimitingOptions
{
    public int QueueLimit { get; set; }
    public int PermitLimit { get; set; }

    public ConcurrencyLimiterOptions Options => new()
    {
        QueueLimit = QueueLimit,
        PermitLimit = PermitLimit,
    };
}
