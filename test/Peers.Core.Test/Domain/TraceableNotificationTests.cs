using Peers.Core.Domain;
using Peers.Core.Identity;
using Microsoft.AspNetCore.Http;

namespace Peers.Core.Test.Domain;

public class TraceableNotificationTests
{
    [Fact]
    public void Ctor_should_initialize_properties()
    {
        // Arrange
        var traceId = "test-trace-id";
        var contextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        contextAccessor
            .Setup(p => p.HttpContext)
            .Returns(new DefaultHttpContext { TraceIdentifier = traceId })
            .Verifiable();

        var identity = new IdentityInfo(contextAccessor.Object);

        // Act
        var notification = new MyNotification(identity);

        // Assert
        Assert.Equal(traceId, notification.TraceIdentifier);
        contextAccessor.VerifyAll();
    }

    private class MyNotification : TraceableNotification
    {
        public MyNotification(IIdentityInfo identity) : base(identity) { }
    }
}
