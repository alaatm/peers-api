using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Peers.Core.Security.Jwt;

public static class TokenIdResolver
{
    public const string TokenIdQueryKey = "bti";
    private static readonly CompositeFormat _tokenIdKeyFormat = CompositeFormat.Parse("callback:jwt:{0}");

    /// <summary>
    /// Sets the cached JWT from the given tokenId query parameter. This is used for payment processing callbacks
    /// so that we can retrieve the JWT from the cache and use it to set the original user identity to process the payment properly.
    /// </summary>
    /// <param name="context">The message received context.</param>
    /// <returns></returns>
    public static Task Resolve([NotNull] MessageReceivedContext context)
    {
        var request = context.Request;

        if (!request.Headers.ContainsKey("Authorization") &&
            request.Path.StartsWithSegments("/payments", StringComparison.OrdinalIgnoreCase) &&
            request.Query.TryGetValue(TokenIdQueryKey, out var tokenIdQueryValue) &&
            tokenIdQueryValue.ToString() is { Length: > 0 } tokenId)
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            context.Token = cache.Get<string>(GetTokenIdCacheKey(tokenId));
        }

        return Task.CompletedTask;
    }

    public static string GenerateTokenIdCacheKey(out string tokenId)
        => GetTokenIdCacheKey(tokenId = Guid.NewGuid().ToString("N"));

    public static string GetTokenIdCacheKey(string tokenId)
        => string.Format(CultureInfo.InvariantCulture, _tokenIdKeyFormat, tokenId);
}
