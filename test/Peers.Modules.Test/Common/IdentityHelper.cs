using Peers.Core.Identity;

namespace Peers.Modules.Test.Common;

public static class IdentityHelper
{
    public static IIdentityInfo Get(string traceId = null)
    {
        var ii = new Mock<IIdentityInfo>(MockBehavior.Loose);
        ii.SetupGet(p => p.TraceIdentifier).Returns(traceId ?? Guid.NewGuid().ToString());
        return ii.Object;
    }
}
