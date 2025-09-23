using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Peers.Modules;

public static class ValueComparers
{
    public static ValueComparer<string[]> StringArrayComparer { get; } = new ValueComparer<string[]>(
        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode(StringComparison.Ordinal))),
        c => c.ToArray());
}
