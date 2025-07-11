using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Mashkoor.Core.Identity;
using Microsoft.AspNetCore.RateLimiting;

namespace Mashkoor.Core.RateLimiting;

public sealed class GenericRateLimiter
{
    public const string PerUserRateLimitPolicyName = "PerUserRateLimit";
    private readonly RateLimitingOptions _config;

    public GenericRateLimiter([NotNull] IConfiguration config)
    {
        _config = new RateLimitingOptions();
        config.GetSection(RateLimitingOptions.SectionName).Bind(_config, o => o.ErrorOnUnknownConfiguration = true);

        if (_config.PerUserRateLimit is null ||
            _config.AnonRateLimit is null ||
            _config.AnonConcurrencyLimit is null)
        {
            throw new InvalidOperationException("Rate limiting options are not configured.");
        }
    }

    public void Configure([NotNull] RateLimiterOptions options)
    {
        options.OnRejected = OnRejectedHandler;

        // Note: PerUserRateLimit should be applied to protected endpoints only.
        // Generally, the identity should be authenticated but not always (invalid or missing JWT)
        // Always ensure protected endpoints are registered under the group that has ".RequireRateLimiting(PerUserRateLimitPolicyName)" call.
        options.AddPolicy(policyName: PerUserRateLimitPolicyName, partitioner: PerUserPartitioner);

        options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
            PartitionedRateLimiter.Create<HttpContext, IPAddress>(httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(GetClientIP(httpContext), _ => _config.AnonRateLimit.Options)),
            PartitionedRateLimiter.Create<HttpContext, IPAddress>(httpContext =>
                RateLimitPartition.GetConcurrencyLimiter(GetClientIP(httpContext), _ => _config.AnonConcurrencyLimit.Options)));
    }

    internal ValueTask OnRejectedHandler(OnRejectedContext context, CancellationToken _)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
            .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
            .RateLimitRejected(context.HttpContext.Request.Path);

        return new ValueTask();
    }

    internal RateLimitPartition<string> PerUserPartitioner(HttpContext httpContext)
    {
        var identity = httpContext
            .RequestServices
            .GetRequiredService<IIdentityInfo>();

        if (!identity.IsAuthenticated)
        {
            // Fall back to per-IP anon rate limit for unauthenticated users
            return RateLimitPartition.GetTokenBucketLimiter(GetClientIP(httpContext).ToString(), _ => _config.AnonRateLimit.Options);
        }

        return RateLimitPartition.GetTokenBucketLimiter(identity.Username!, _ => _config.PerUserRateLimit.Options);
    }

    private static IPAddress GetClientIP(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress ?? /* In tests, RemoteIpAddress is null */ IPAddress.Any;
}
