using System.Net;
using System.Threading.RateLimiting;
using Mashkoor.Core.Identity;
using Mashkoor.Core.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mashkoor.Core.Test.RateLimiting;

public class GenericRateLimiterTests
{
    private static readonly IConfiguration _config = new ConfigurationBuilder()
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

    [Fact]
    public void Ctor_throws_when_options_arent_configured()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => new GenericRateLimiter(config));
        Assert.Equal("Rate limiting options are not configured.", ex.Message);
    }

    [Fact]
    public void Configure_configures_rateLimiterOptions()
    {
        // Arrange
        var genericRateLimiter = new GenericRateLimiter(_config);
        var rateLimiterOptions = new RateLimiterOptions();

        // Act
        genericRateLimiter.Configure(rateLimiterOptions);

        // Assert
        Assert.Equal(genericRateLimiter.OnRejectedHandler, rateLimiterOptions.OnRejected);
        Assert.NotNull(rateLimiterOptions.GlobalLimiter);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OnRejectedHandler_sets_statusCode_and_retryAfter_header_when_metadata_exist_on_the_lease(bool loggingServicesRegistered)
    {
        // Arrange
        var genericRateLimiter = new GenericRateLimiter(_config);

        var httpContext = GetHttpContext(loggingServicesRegistered: loggingServicesRegistered);
        var leaseMoq = new Mock<RateLimitLease>() { CallBase = true };

        object retryAfter = TimeSpan.FromSeconds(30);
        leaseMoq
            .Setup(l => l.TryGetMetadata(MetadataName.RetryAfter.Name, out retryAfter))
            .Returns(true);

        var rejectedContext = new OnRejectedContext
        {
            HttpContext = httpContext,
            Lease = leaseMoq.Object
        };

        // Act
        await genericRateLimiter.OnRejectedHandler(rejectedContext, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status429TooManyRequests, rejectedContext.HttpContext.Response.StatusCode);
        Assert.True(rejectedContext.HttpContext.Response.Headers.ContainsKey("Retry-After"));
        Assert.Equal(((TimeSpan)retryAfter).TotalSeconds.ToString(), rejectedContext.HttpContext.Response.Headers["Retry-After"]);
        Assert.Equal(StatusCodes.Status429TooManyRequests, rejectedContext.HttpContext.Response.StatusCode);
        leaseMoq.VerifyAll();
    }

    [Fact]
    public async Task OnRejectedHandler_sets_statusCode_and_no_retryAfter_header_when_metadata_does_not_exist_on_the_lease()
    {
        // Arrange
        var genericRateLimiter = new GenericRateLimiter(_config);

        var httpContext = GetHttpContext();
        var leaseMoq = new Mock<RateLimitLease>(MockBehavior.Strict) { CallBase = true };

        object retryAfter = null;
        leaseMoq
            .Setup(l => l.TryGetMetadata(MetadataName.RetryAfter.Name, out retryAfter))
            .Returns(false);

        var rejectedContext = new OnRejectedContext
        {
            HttpContext = httpContext,
            Lease = leaseMoq.Object
        };

        // Act
        await genericRateLimiter.OnRejectedHandler(rejectedContext, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status429TooManyRequests, rejectedContext.HttpContext.Response.StatusCode);
        Assert.False(rejectedContext.HttpContext.Response.Headers.ContainsKey("Retry-After"));
        Assert.Equal(StatusCodes.Status429TooManyRequests, rejectedContext.HttpContext.Response.StatusCode);
        leaseMoq.VerifyAll();
    }

    [Fact]
    public void PerUserPartitioner_returns_username_partition_for_authenticated_user()
    {
        // Arrange
        var genericRateLimiter = new GenericRateLimiter(_config);
        var identityMoq = GetIdentityMoq(true, out var username);
        var httpContext = GetHttpContext(identityMoq.Object);

        // Act
        var partition = genericRateLimiter.PerUserPartitioner(httpContext);

        // Assert
        Assert.Equal(username, partition.PartitionKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("5.5.5.5")]
    public void PerUserPartitioner_returns_ipAddress_partition_for_unauthenticated_user(string ipAddress)
    {
        // Arrange
        var genericRateLimiter = new GenericRateLimiter(_config);
        var identityMoq = GetIdentityMoq(false, out _);
        var httpContext = GetHttpContext(identityMoq.Object, ipAddress);

        // Act
        var partition = genericRateLimiter.PerUserPartitioner(httpContext);

        // Assert
        Assert.Equal(ipAddress ?? IPAddress.Any.ToString(), partition.PartitionKey);
    }

    private static DefaultHttpContext GetHttpContext(IIdentityInfo identity = null, string ipAddress = null, bool loggingServicesRegistered = false)
    {
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection();
        if (loggingServicesRegistered)
        {
            services.AddLogging();
        }
        if (identity != null)
        {
            services.AddSingleton(identity);
        }
        httpContext.RequestServices = services.BuildServiceProvider();

        if (ipAddress is not null)
        {
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        }

        return httpContext;
    }

    private static Mock<IIdentityInfo> GetIdentityMoq(bool isAuthenticated, out string username)
    {
        username = null;
        var identityMoq = new Mock<IIdentityInfo>(MockBehavior.Strict);

        if (isAuthenticated)
        {
            username = "testuser";
            identityMoq
                .SetupGet(i => i.Username)
                .Returns(username)
                .Verifiable();
        }

        identityMoq
            .SetupGet(i => i.IsAuthenticated)
            .Returns(isAuthenticated)
            .Verifiable();

        return identityMoq;
    }
}
