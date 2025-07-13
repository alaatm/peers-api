using System.Diagnostics.CodeAnalysis;
using Mashkoor.Core.Security.Totp.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Mashkoor.Core.Security.Totp;

/// <summary>
/// A time-based one-time password provider.
/// </summary>
public sealed class TotpTokenProvider : TotpTokenProviderBase
{
    private readonly IMemoryCache _cache;

    public TotpTokenProvider(
        TotpConfig config,
        TimeProvider timeProvider,
        IMemoryCache cache) : base(config, timeProvider) => _cache = cache;

    protected override void SetCacheValue(string key, string value, TimeSpan expiration) => _cache.Set(key, value, expiration);

    protected override bool TryGetCacheValue(string key, [NotNullWhen(true)] out object? value) => _cache.TryGetValue(key, out value);
}
