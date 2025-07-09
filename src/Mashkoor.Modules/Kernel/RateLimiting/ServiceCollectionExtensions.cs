using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

namespace Mashkoor.Modules.Kernel.RateLimiting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        [NotNull] IConfiguration config)
    {
        var rateLimitingOpts = new RateLimitingOptions();
        var perUserRateLimitOpts = rateLimitingOpts.PerUserRateLimit;
        var anonRateLimitOpts = rateLimitingOpts.AnonRateLimit;
        var anonConcurrencyLimitOpts = rateLimitingOpts.AnonConcurrencyLimit;

        config.GetSection(RateLimitingOptions.SectionName).Bind(rateLimitingOpts, o => o.ErrorOnUnknownConfiguration = true);

        if (rateLimitingOpts.PerUserRateLimit is null ||
            rateLimitingOpts.AnonRateLimit is null ||
            rateLimitingOpts.AnonConcurrencyLimit is null)
        {
            throw new InvalidOperationException("Rate limiting options are not configured.");
        }

        return services.AddRateLimiter(p =>
        {
            p.OnRejected = (context, _) =>
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
            };

            // Note: PerUserRateLimit is applied to protected endpoints only.
            // Generally, the identity should be authenticated but not always (invalid or missing JWT)
            // Always ensure protected endpoints are registered under the group that has ".RequireRateLimiting("PerUserRateLimit")" call.
            p.AddPolicy(policyName: "PerUserRateLimit", partitioner: httpContext =>
            {
                var identity = httpContext
                    .RequestServices
                    .GetRequiredService<IIdentityInfo>();

                if (!identity.IsAuthenticated)
                {
                    // Fall back to per-IP anon rate limit for unauthenticated users
                    return RateLimitPartition.GetTokenBucketLimiter(GetClientIP(httpContext).ToString(), _ => rateLimitingOpts.AnonRateLimit.Options);
                }

                return RateLimitPartition.GetTokenBucketLimiter(identity.Username!, _ => rateLimitingOpts.PerUserRateLimit.Options);
            });

            p.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, IPAddress>(httpContext =>
                    RateLimitPartition.GetTokenBucketLimiter(GetClientIP(httpContext), _ => rateLimitingOpts.AnonRateLimit.Options)),
                PartitionedRateLimiter.Create<HttpContext, IPAddress>(httpContext =>
                    RateLimitPartition.GetConcurrencyLimiter(GetClientIP(httpContext), _ => rateLimitingOpts.AnonConcurrencyLimit.Options)));
        });
    }

    private static IPAddress GetClientIP(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress ?? /* In tests, RemoteIpAddress is null */ IPAddress.Any;
}
