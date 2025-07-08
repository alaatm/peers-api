using Build.Common;
using Cake.Core;

namespace Build;

public class BuildContext : BuildContextBase
{
    public BuildContext(ICakeContext context)
        : base(context)
    {
    }
}
