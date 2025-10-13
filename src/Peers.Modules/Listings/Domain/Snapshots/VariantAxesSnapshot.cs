using System.Diagnostics;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain;

namespace Peers.Modules.Listings.Domain.Snapshots;

/// <summary>
/// Canonical, serialized set of variant axes for a listing.
/// </summary>
/// <param name="SnapshotId">A stable identifier for this snapshot.</param>
/// <param name="Version">Version of the snapshot structure; increment when you regenerate axes snapshot structure.</param>
/// <param name="CreatedAt">Timestamp when the snapshot was created.</param>
/// <param name="Axes">The list of variant axes and their offered choices, in canonical order.</param>
[DebuggerDisplay("{D,nq}")]
public sealed partial record VariantAxesSnapshot(
    string SnapshotId,
    int Version,
    DateTimeOffset CreatedAt,
    List<VariantAxisSnapshot> Axes) : IDebuggable
{
    internal static VariantAxesSnapshot Create(int version)
        => new(Guid.NewGuid().ToString(), version, DateTime.UtcNow, []);

    internal List<VariantAxis> ToRuntime(ProductType productType)
    {
        var axes = new List<VariantAxis>(Axes.Count);
        foreach (var axis in Axes)
        {
            axes.Add(axis.ToRuntime(productType));
        }
        return axes;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"VAxesSnap - {SnapshotId.ToString().Split('-').Last()} (v{Version}) | {Axes.Count} axes";
}
