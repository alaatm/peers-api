using System.Diagnostics;
using Peers.Core.Domain.Errors;

namespace Peers.Modules.Listings.Domain.Snapshots;

/// <summary>
/// References the axes snapshot + the choices this variant selected.
/// </summary>
/// <param name="SnapshotId">Must match Listing.VariantAxesSnapshot.SnapshotId</param>
/// <param name="Selections">The selected choices on each axis.</param>
[DebuggerDisplay("{D,nq}")]
public sealed partial record VariantSelectionSnapshot(
    string SnapshotId,
    List<AxisSelectionRef> Selections) : IDebuggable
{
    internal static VariantSelectionSnapshot Create(string snapshotId)
        => new(snapshotId, []);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"VSSnap - {SnapshotId.ToString().Split('-').Last()} | {Selections.Count} selections";
}
