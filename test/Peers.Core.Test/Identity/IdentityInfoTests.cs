using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Peers.Core.Identity;

namespace Peers.Core.Test.Identity;

public class IdentityInfoTests
{
    [Fact]
    public void Ctor_initializes_instance()
    {
        // Arrange
        var ip = "1.1.1.1";
        var traceId = Guid.NewGuid().ToString();

        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(SharedStubs.BuildHttpContext(ip, traceId: traceId));

        // Act
        var ii = new IdentityInfo(httpContextAccessorMoq.Object);

        Assert.Equal(traceId, ii.TraceIdentifier);
        Assert.Equal(ip, ii.Ip);
        Assert.False(ii.IsAuthenticated);
        Assert.Equal(0, ii.Id);
        Assert.Null(ii.Username);
        Assert.False(ii.IsInRole(""));

        httpContextAccessorMoq.VerifyAll();
    }

    [Fact]
    public void Ctor_handles_null_context()
    {
        // Arrange
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns((HttpContext)null);

        // Act
        var ii = new IdentityInfo(httpContextAccessorMoq.Object);

        Assert.Null(ii.TraceIdentifier);
        Assert.Null(ii.Ip);
        Assert.False(ii.IsAuthenticated);
        Assert.Equal(0, ii.Id);
        Assert.Null(ii.Username);
        Assert.False(ii.IsInRole(""));

        httpContextAccessorMoq.VerifyAll();
    }

    [Fact]
    public void Ctor_initializes_identity_info()
    {
        // Arrange
        var userId = 999;
        var username = "alaatm";
        var roles = new[] { "admin" };
        var claims = new Claim[] { new(ClaimTypes.Email, "x@y.z") };
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(SharedStubs.BuildHttpContext(null, userId, username, roles, claims));

        // Act
        var ii = new IdentityInfo(httpContextAccessorMoq.Object);

        Assert.NotNull(ii.TraceIdentifier);
        Assert.Null(ii.Ip);
        Assert.True(ii.IsAuthenticated);
        Assert.Equal(userId, ii.Id);
        Assert.Equal(userId, ii.Id); // Ensure Id is cached
        Assert.Equal(username, ii.Username);
        Assert.True(ii.IsInRole(roles[0]));
        Assert.False(ii.IsInRole("???"));

        httpContextAccessorMoq.VerifyAll();
    }
}
