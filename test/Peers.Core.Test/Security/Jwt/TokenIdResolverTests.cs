using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Peers.Core.Security.Jwt;

namespace Peers.Core.Test.Security.Jwt;

public class TokenIdResolverTests
{
    [Fact]
    public async Task No_ops_when_auth_header_is_present()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token";
        var msgReceivedContext = GetMessageReceivedContext(context);

        // Act
        await TokenIdResolver.Resolve(msgReceivedContext);

        // Assert
        Assert.Null(msgReceivedContext.Token);
    }

    [Fact]
    public async Task No_ops_when_path_does_not_start_with_payments()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/other";
        var msgReceivedContext = GetMessageReceivedContext(context);

        // Act
        await TokenIdResolver.Resolve(msgReceivedContext);

        // Assert
        Assert.Null(msgReceivedContext.Token);
    }

    [Fact]
    public async Task No_ops_when_tokenIdQuery_is_not_set()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/payments/tokenize";
        var msgReceivedContext = GetMessageReceivedContext(context);

        // Act
        await TokenIdResolver.Resolve(msgReceivedContext);

        // Assert
        Assert.Null(msgReceivedContext.Token);
    }

    [Fact]
    public async Task No_ops_when_tokenIdQuery_is_empty()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/payments/tokenize";
        context.Request.QueryString = new QueryString($"?{TokenIdResolver.TokenIdQueryKey}=");
        var msgReceivedContext = GetMessageReceivedContext(context);

        // Act
        await TokenIdResolver.Resolve(msgReceivedContext);

        // Assert
        Assert.Null(msgReceivedContext.Token);
    }

    [Fact]
    public async Task Sets_cached_bearerToken_when_all_conditions_are_met()
    {
        // Arrange
        var bearerToken = Guid.NewGuid().ToString();
        var cacheKey = TokenIdResolver.GenerateTokenIdCacheKey(out var tokenId);

        var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set(cacheKey, bearerToken);

        var context = new DefaultHttpContext()
        {
            RequestServices = new ServiceCollection()
                .AddSingleton<IMemoryCache>(cache)
                .BuildServiceProvider(),
        };
        context.Request.Path = "/payments/tokenize";
        context.Request.QueryString = new QueryString($"?{TokenIdResolver.TokenIdQueryKey}={tokenId}");
        var msgReceivedContext = GetMessageReceivedContext(context);

        // Act
        await TokenIdResolver.Resolve(msgReceivedContext);

        // Assert
        Assert.Equal(bearerToken, msgReceivedContext.Token);
    }

    private static MessageReceivedContext GetMessageReceivedContext(HttpContext context)
    {
        var authScheme = new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme, typeof(JwtBearerHandler));
        return new MessageReceivedContext(context, authScheme, new());
    }
}
