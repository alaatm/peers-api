using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Mashkoor.Core.AzureServices.AppInsights;
using System.Security.Claims;
using System.Security.Principal;

namespace Mashkoor.Core.Test.AzureServices.AppInsights;

public class TelemetryEnrichmentTests
{
    [Fact]
    public void Enriches_authenticatedUserId_with_authenticated_identity()
    {
        // Arrange
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(BuildHttpContext("username")).Verifiable();

        var te = new TelemetryEnrichment(httpContextAccessorMoq.Object);
        var testTelemetry = new TestTelemetry();

        // Act
        te.Initialize(testTelemetry);

        // Assert
        Assert.Equal("username", testTelemetry.Context.User.AuthenticatedUserId);
        httpContextAccessorMoq.Verify();
    }

    [Fact]
    public void Sets_authenticatedUserId_to_null_when_no_auth_identity_exist()
    {
        // Arrange
        var httpContextAccessorMoq = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        httpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(BuildHttpContext()).Verifiable();

        var te = new TelemetryEnrichment(httpContextAccessorMoq.Object);
        var testTelemetry = new TestTelemetry();

        // Act
        te.Initialize(testTelemetry);

        // Assert
        Assert.Null(testTelemetry.Context.User.AuthenticatedUserId);
        httpContextAccessorMoq.Verify();
    }

    private static DefaultHttpContext BuildHttpContext(string username = null)
    {
        var httpContext = new DefaultHttpContext();

        var fc = new FeatureCollection();
        fc[typeof(RequestTelemetry)] = new RequestTelemetry();

        httpContext.Initialize(fc);
        if (username is not null)
        {
            var identity = new GenericIdentity(username);
            var principal = new GenericPrincipal(identity, []);
            httpContext.User = principal;
        }
        else
        {
            httpContext.User = new ClaimsPrincipal([]);
        }

        return httpContext;
    }

    private class TestTelemetry : ITelemetry
    {
        public DateTimeOffset Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TelemetryContext Context { get; } = new();

        public IExtension Extension { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Sequence { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ITelemetry DeepClone() => throw new NotImplementedException();
        public void Sanitize() => throw new NotImplementedException();
        public void SerializeData(ISerializationWriter serializationWriter) => throw new NotImplementedException();
    }
}
