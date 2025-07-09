using Mashkoor.Core.Domain.Rules;
using Mashkoor.Modules.Test.SharedClasses;

namespace Mashkoor.Modules.Test;

public class DomainEntityTestBase
{
    public DomainEntityTestBase()
        => BusinessRule.StringLocalizerFactory ??= MockBuilder.GetLocalizerFactoryMoq().Object;
}
