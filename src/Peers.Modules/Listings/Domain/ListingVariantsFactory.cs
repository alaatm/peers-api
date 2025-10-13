using System.Diagnostics;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Snapshots;

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
    /// <param name="axesSnapshot">Outputs a snapshot of the variant axes used to generate the variants.</param>
    public static List<ListingVariant> GenerateVariants(
        Listing listing,
        IReadOnlyList<VariantAxis> axes,
        out VariantAxesSnapshot axesSnapshot)
    {
        axesSnapshot = VariantAxesSnapshot.Create(listing.Version);

        if (axes.Count == 0)
        {
            return [ListingVariant.CreateDefault(listing, VariantSelectionSnapshot.Create(axesSnapshot))];
        }

        // Transform each axis into a stream of picks
        var streams = new List<List<AxisPick>>(axes.Count);

        foreach (var axis in axes) // Axes already sorted canonically
        {
            var stream = new List<AxisPick>(axis.Cardinality);
            foreach (var choice in axis.Choices) // Choices already deduped & sorted
            {
                stream.Add(choice.ToPick(axis.Definition));
            }

            streams.Add(stream);
            axesSnapshot.Axes.Add(axis.ToSnapshot());
        }

        // Cartesian across streams → flat lists of (def, value) pairs
        var variants = new List<ListingVariant>(EstimateSkuCount(axes));

        foreach (var pick in Cartesian(streams))
        {
            var selectionRefs = BuildSelectionRefs(pick, axesSnapshot);
            var selectionSnapshot = new VariantSelectionSnapshot(
                axesSnapshot.SnapshotId,
                selectionRefs);

            variants.Add(ListingVariant.Create(listing, pick, selectionSnapshot));
        }

        return variants;
    }

    public static int EstimateSkuCount(IReadOnlyList<VariantAxis> axes)
    {
        if (axes.Count == 0)
        {
            // The default variant always exists, even with no axes.
            return 1;
        }

        var count = 1;
        foreach (var (_, values) in axes)
        {
            checked
            { count *= values.Count; }
        }

        return count;
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

    // Performs the following:
    // a. Buckets the AxisSelections by axis (for group members, it buckets by the group key),
    // c. Finds the matching AxisChoiceSnapshot on that axis, and emits one AxisSelectionRef per axis.
    private static List<AxisSelectionRef> BuildSelectionRefs(
        List<AxisSelection> pick,
        VariantAxesSnapshot snapshot)
    {
        // Bucket by axis key: group members → group key; others → their own def key
        var buckets = new Dictionary<string, List<AxisSelection>>(StringComparer.Ordinal);
        foreach (var selection in pick)
        {
            var axisKey =
                selection.Definition is NumericAttributeDefinition num && num.GroupDefinition is not null
                    ? num.GroupDefinition.Key       // group axis
                    : selection.Definition.Key;     // single axis

            if (!buckets.TryGetValue(axisKey, out var list))
            {
                buckets[axisKey] = list = [];
            }

            list.Add(selection);
        }

        var refs = new List<AxisSelectionRef>(buckets.Count);

        foreach (var (axisKey, selections) in buckets)
        {
            // Locate axis in the snapshot
            var axisSnap = snapshot.Axes.Single(a => a.DefinitionKey == axisKey);
            // Locate choice on that axis matching the selection(s)
            var choiceSnap = FindMatchingChoiceSnap(axisSnap, selections);
            refs.Add(new AxisSelectionRef(axisKey, choiceSnap.Key));
        }

        return refs;
    }

    private static AxisChoiceSnapshot FindMatchingChoiceSnap(VariantAxisSnapshot axisSnap, List<AxisSelection> selections)
    {
        if (!axisSnap.IsGroup)
        {
            // Single axis must have exactly one selection
            Debug.Assert(selections.Count == 1);
            var choiceToMatch = selections[0].Choice;

            foreach (var c in axisSnap.Choices)
            {
                switch (choiceToMatch)
                {
                    case { EnumOption: not null } when c.EnumOptionCode == choiceToMatch.EnumOption!.Code:
                    case { LookupOption: not null } when c.LookupOptionCode == choiceToMatch.LookupOption!.Code:
                    case { NumericValue: not null } when c.NumericValue == choiceToMatch.NumericValue:
                        return c;
                    default:
                        break;
                }
            }

            var error = $"No matching choice found on axis '{axisSnap.DefinitionKey}' for the provided selection.";
            Debug.Fail(error);
            throw new InvalidOperationException(error);
        }
        else
        {
            // Group axis must have at least 2 selections (one per member).
            Debug.Assert(selections.Count >= 2);

            foreach (var c in axisSnap.Choices)
            {
                Debug.Assert(c.GroupMembers is not null);

                var allMatch = true;
                var members = c.GroupMembers;

                // All members must match by value in the same order. Def keys are expected to match regardless.

                for (var i = 0; i < selections.Count; i++)
                {
                    Debug.Assert(selections[i].Definition.Key == members[i].MemberDefinitionKey);

                    if (selections[i].Definition.Key != members[i].MemberDefinitionKey ||
                        selections[i].Choice.NumericValue != members[i].Value)
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch)
                {
                    return c;
                }
            }

            var error = $"No matching composite choice found on group axis '{axisSnap.DefinitionKey}' for the provided selections.";
            Debug.Fail(error);
            throw new InvalidOperationException(error);
        }
    }
}
