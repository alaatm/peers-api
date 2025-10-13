namespace Peers.Modules.Listings.Domain.Snapshots;

/// <summary>
/// References the axes snapshot + the choices this variant selected.
/// </summary>
/// <param name="SnapshotId">Must match Listing.VariantAxesSnapshot.SnapshotId</param>
/// <param name="Selections">The selected choices on each axis.</param>
public sealed record VariantSelectionSnapshot(
    string SnapshotId,
    List<AxisSelectionRef> Selections)
{
    internal static VariantSelectionSnapshot Create(VariantAxesSnapshot axesSnapshot)
        => new(axesSnapshot.SnapshotId, []);
}
