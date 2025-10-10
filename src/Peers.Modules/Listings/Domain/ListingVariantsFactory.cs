using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Provides factory methods for generating all possible combinations of listing variants based on attribute definitions
/// and their options.
/// </summary>
internal static class ListingVariantsFactory
{
    /// <summary>
    /// Compose SKUs from validated, canonical axes. Assumes:
    /// - <paramref name="axes"/> is sorted by AttributeDefinition.Position then Key,
    /// - each value list is deduped and sorted,
    /// - each AxisValue has exactly one non-null payload:
    ///   EnumOption | LookupOption | Numeric | Group (and Group contains member defs in canonical order).
    /// </summary>
    /// <param name="listing">The listing for which the variants are being generated.</param>
    /// <param name="axes">The input list where each entry is a pair of an attribute definition and a list of its possible axis values.</param>
    public static List<ListingVariant> GenerateVariants(
        Listing listing,
        IReadOnlyList<KeyValuePair<AttributeDefinition, List<AxisValue>>> axes)
    {
        if (axes.Count == 0)
        {
            return [ListingVariant.CreateDefault(listing)];
        }

        // Transform each axis into a stream of picks
        var streams = new List<List<AxisPick>>(axes.Count);

        foreach (var (def, values) in axes)
        {
            var stream = new List<AxisPick>(values.Count);

            foreach (var v in values)
            {
                if (v.Group is not null)
                {
                    // Composite/group: a single pick with N member items
                    var pick = new AxisPick(v.Group.Count);
                    foreach (var m in v.Group) // members already in canonical order
                    {
                        pick.Add((m.Def, new SingleAxisValue(Numeric: m.Value)));
                    }

                    stream.Add(pick);
                }
                else
                {
                    // Single axis: one item pick
                    stream.Add([(def, new SingleAxisValue(v.EnumOption, v.LookupOption, v.Numeric))]);
                }
            }

            streams.Add(stream);
        }

        // Cartesian across streams → flat lists of (def, value) pairs
        var variants = new List<ListingVariant>(EstimateSkuCount(axes));

        foreach (var pairs in Cartesian(streams))
        {
            variants.Add(ListingVariant.Create(listing, pairs));
        }

        return variants;
    }

    // Iterative cartesian: each stream is a list of AxisPick; each result is a flat list of (def, value)
    private static IEnumerable<AxisPick> Cartesian(List<List<AxisPick>> streams)
    {
        var acc = new AxisPick[] { [] };

        foreach (var stream in streams)
        {
            var next = new List<AxisPick>(acc.Length * Math.Max(1, stream.Count));

            foreach (var partial in acc)
            {
                foreach (var pick in stream)
                {
                    var merged = new AxisPick(partial.Count + pick.Count);
                    merged.AddRange(partial);
                    // append this axis’ contribution (1 for single, N for group)
                    for (var i = 0; i < pick.Count; i++)
                    {
                        merged.Add(pick[i]);
                    }

                    next.Add(merged);
                }
            }

            acc = [.. next];
        }

        foreach (var combo in acc)
        {
            yield return combo;
        }
    }

    public static int EstimateSkuCount(
        IReadOnlyList<KeyValuePair<AttributeDefinition, List<AxisValue>>> axis)
    {
        if (axis.Count == 0)
        {
            // The default variant always exists, even with no axes.
            return 1;
        }

        var count = 1;
        foreach (var (_, values) in axis)
        {
            checked
            { count *= values.Count; }
        }

        return count;
    }

    public record SingleAxisValue(
        EnumAttributeOption? EnumOption = null,
        LookupValue? LookupOption = null,
        decimal? Numeric = null);

    public sealed record AxisValue(
        EnumAttributeOption? EnumOption = null,
        LookupValue? LookupOption = null,
        decimal? Numeric = null,
        List<MemberAxis>? Group = null) : SingleAxisValue(EnumOption, LookupOption, Numeric);

    public sealed record MemberAxis(NumericAttributeDefinition Def, decimal Value);

    public sealed class AxisPick : List<(AttributeDefinition Def, SingleAxisValue Value)>
    {
        public AxisPick() { }
        public AxisPick(int capacity) : base(capacity) { }
    }
}
