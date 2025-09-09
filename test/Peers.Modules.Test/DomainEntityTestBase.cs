using Peers.Core.Domain.Rules;
using Peers.Modules.Test.SharedClasses;

namespace Peers.Modules.Test;

public class DomainEntityTestBase
{
    public DomainEntityTestBase()
        => BusinessRule.StringLocalizerFactory ??= MockBuilder.GetLocalizerFactoryMoq().Object;
}
