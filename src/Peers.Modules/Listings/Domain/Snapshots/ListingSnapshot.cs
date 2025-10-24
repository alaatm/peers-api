using System.Diagnostics;
using Peers.Modules.Catalog.Domain;

namespace Peers.Modules.Listings.Domain.Snapshots;

/// <summary>
/// Represents an immutable snapshot of a product listing, including its header information, variant axes, and metadata
/// at a specific point in time.
/// </summary>
/// <param name="Version">The version of the snapshot.</param>
/// <param name="SnapshotId">A stable identifier for this snapshot.</param>
/// <param name="CreatedAt">Timestamp when the snapshot was created.</param>
/// <param name="UpdatedAt">Timestamp when the snapshot was last updated.</param>
/// <param name="Attributes">The list of header attributes and their values.</param>
/// <param name="Axes">The list of variant axes and their offered choices, in canonical order.</param>
[DebuggerDisplay("{D,nq}")]
public sealed partial record ListingSnapshot(
    int Version,
    string SnapshotId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<HeaderAttrSnapshot> Attributes,
    List<VariantAxisSnapshot> Axes)
{
    internal static ListingSnapshot Create(DateTime date) => new(
        1,
        Guid.NewGuid().ToString(),
        date,
        date,
        [],
        []);

    internal ListingSnapshot Update(
        List<VariantAxisSnapshot> axes,
        DateTime date) => this with
        {
            Version = Version + 1,
            SnapshotId = Guid.NewGuid().ToString(),
            UpdatedAt = date,
            Axes = axes
        };

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
    public string D => $"LSnap - {SnapshotId.ToString().Split('-').Last()} (v{Version}) - {Attributes.Count} attrs | {Axes.Count} axes";
}
