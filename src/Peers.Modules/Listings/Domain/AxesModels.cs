using System.Diagnostics;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// One offered value for a variant axis as received/constructed after validation.
/// For single axes, one of <see cref="EnumOption"/>, <see cref="LookupOption"/>, or <see cref="NumericValue"/> is set.
/// For composite/group axes, <see cref="GroupMembers"/> is set and the scalar fields are null.
/// </summary>
/// <param name="Key">
/// A stable, unique key within the axis for this choice within the axis.
/// For enum -> The option code. (e.g., "red", "large").
/// For lookup -> The option code. (e.g., "us", "ca").
/// For numeric -> The numeric value as a string. (e.g., "150").
/// For group -> Canonical "memberKey=value" pairs joined by commas (ordered by member Position→Key), e.g., "width=150,length=400".
/// </param>
/// <param name="EnumOption">The enum option to use for the axis value; null if not using an enum.</param>
/// <param name="LookupOption">The lookup option to use for the axis value; null if not using a lookup.</param>
/// <param name="NumericValue">The numeric value to use for the axis; null if not using a numeric.</param>
/// <param name="GroupMembers">A list of group members representing the axis value; null if not using a group.</param>
public sealed record AxisChoice(
    string Key,
    EnumAttributeOption? EnumOption = null,
    LookupOption? LookupOption = null,
    decimal? NumericValue = null,
    List<AxisChoice.GroupMember>? GroupMembers = null)
{
    /// <summary>
    /// A numeric value bound to a specific member of a group/composite axis (e.g., Width = 150).
    /// Members are ordered in the group’s canonical member order (Position, then Key).
    /// </summary>
    /// <param name="MemberDefinition">The definition of the numeric attribute.</param>
    /// <param name="Value">The numeric value assigned to the group member.</param>
    public sealed record GroupMember(
        NumericAttributeDefinition MemberDefinition,
        decimal Value);

    /// <summary>
    /// Converts this choice into a normalized pick for cartesian expansion.
    /// For group axes, returns one selection per member; for single axes, returns exactly one selection.
    /// </summary>
    /// <param name="axisDefinition">
    /// The attribute definition for this axis. If it is a <see cref="GroupAttributeDefinition"/>,
    /// <see cref="GroupMembers"/> must be non-null and aligned to the group's canonical member order.
    /// </param>
    internal AxisPick ToPick(AttributeDefinition axisDefinition)
    {
        if (axisDefinition is GroupAttributeDefinition)
        {
            if (GroupMembers is null)
            {
                throw new InvalidOperationException($"Axis '{axisDefinition.Key}' is a group; GroupMembers must be provided.");
            }

            var pick = new AxisPick(GroupMembers.Count);
            foreach (var m in GroupMembers)
            {
                // Each member is a numeric attribute; flatten to a normalized numeric choice
                pick.Add(new AxisSelection(
                    m.MemberDefinition,
                    new NormalizedAxisChoice(NumericValue: m.Value)
                ));
            }

            return pick;
        }

        // Single axis: exactly one of the scalar fields must be set
        var set =
            (EnumOption is not null ? 1 : 0) +
            (LookupOption is not null ? 1 : 0) +
            (NumericValue is not null ? 1 : 0);

        if (set != 1)
        {
            throw new InvalidOperationException($"Axis '{axisDefinition.Key}' must set exactly one of EnumOption, LookupOption, or NumericValue.");
        }

        return
        [
            new AxisSelection(
                axisDefinition,
                new NormalizedAxisChoice(EnumOption, LookupOption, NumericValue)
            )
        ];
    }

    // Comparison method for sorting
    internal static int EnumComparison(AxisChoice a, AxisChoice b)
    {
        Debug.Assert(a.EnumOption is not null && b.EnumOption is not null);
        return a.EnumOption!.Position.CompareTo(b.EnumOption!.Position);
    }
    internal static int LookupComparison(AxisChoice a, AxisChoice b)
    {
        Debug.Assert(a.LookupOption is not null && b.LookupOption is not null);
        return string.Compare(a.LookupOption!.Code, b.LookupOption!.Code, StringComparison.Ordinal);
    }
    internal static int NumericComparison(AxisChoice a, AxisChoice b)
    {
        Debug.Assert(a.NumericValue is not null && b.NumericValue is not null);
        return a.NumericValue!.Value.CompareTo(b.NumericValue!.Value);
    }
    internal static int GroupComparison(AxisChoice a, AxisChoice b)
    {
        Debug.Assert(a.GroupMembers is not null && b.GroupMembers is not null);
        Debug.Assert(a.GroupMembers!.Count == b.GroupMembers!.Count);
        for (var i = 0; i < a.GroupMembers!.Count; i++)
        {
            var cmp = a.GroupMembers![i].Value.CompareTo(b.GroupMembers![i].Value);
            if (cmp != 0)
            {
                return cmp;
            }
        }
        return 0;
    }
    internal static int Comparison(AxisChoice a, AxisChoice b)
    {
        if (a.EnumOption is not null && b.EnumOption is not null)
        {
            return EnumComparison(a, b);
        }
        else if (a.LookupOption is not null && b.LookupOption is not null)
        {
            return LookupComparison(a, b);
        }
        else if (a.NumericValue is not null && b.NumericValue is not null)
        {
            return NumericComparison(a, b);
        }
        else if (a.GroupMembers is not null && b.GroupMembers is not null)
        {
            return GroupComparison(a, b);
        }

        Debug.Fail("Cannot compare AxisChoice instances of different types or with mismatched fields.");
        throw new InvalidOperationException("Cannot compare AxisChoice instances of different types or with mismatched fields.");
    }
}

/// <summary>
/// An atomic, normalized choice on a single attribute definition (no groups).
/// Exactly one of <see cref="EnumOption"/>, <see cref="LookupOption"/>, or <see cref="NumericValue"/> is set.
/// Used in cartesian expansion and when building <see cref="ListingVariant"/>.
/// An <see cref="AxisChoice"/> can expand to one or more of these if it represents a group, otherwise it maps 1:1.
/// </summary>
/// <param name="EnumOption">An optional enumerated attribute option representing a predefined axis value. Specify this parameter to select an
/// axis value from a known set of enumeration options.</param>
/// <param name="LookupOption">An optional lookup option used to reference an axis value by key or external mapping. Provide this parameter to
/// select an axis value based on a lookup mechanism.</param>
/// <param name="NumericValue">An optional numeric value representing the axis choice as a decimal. Use this parameter to specify a custom axis
/// value directly.</param>
public record NormalizedAxisChoice(
    EnumAttributeOption? EnumOption = null,
    LookupOption? LookupOption = null,
    decimal? NumericValue = null);

/// <summary>
/// Represents a variant axis: the attribute definition and its offered, canonicalized choices.
/// For single axes, each <see cref="AxisChoice"/> has one of EnumOption/LookupOption/NumericValue set.
/// For group axes, each <see cref="AxisChoice"/> has <see cref="AxisChoice.GroupMembers"/> set (others null).
/// Choices are expected to be validated, de-duplicated, and sorted upstream.
/// </summary>
/// <param name="Definition">The attribute definition associated with this variant axis.</param>
/// <param name="Choices">The list of offered, canonicalized choices for this axis.</param>
public sealed record VariantAxis(AttributeDefinition Definition, List<AxisChoice> Choices)
{
    /// <summary>
    /// Number of offered values (or tuples for groups) on this axis.
    /// </summary>
    public int Cardinality => Choices.Count;

    /// <summary>
    /// Returns true if this axis is a composite/group axis.
    /// </summary>
    public bool IsGroup => Definition is GroupAttributeDefinition;

    internal VariantAxisSnapshot ToSnapshot()
    {
        var choices = new List<AxisChoiceSnapshot>(Cardinality);

        foreach (var c in Choices)
        {
            if (c.GroupMembers is not null)
            {
                var members = new List<AxisChoiceSnapshot.GroupMemberSnapshot>(c.GroupMembers.Count);
                foreach (var m in c.GroupMembers)
                {
                    members.Add(new AxisChoiceSnapshot.GroupMemberSnapshot(
                        MemberDefinitionKey: m.MemberDefinition.Key,
                        Value: m.Value));
                }

                choices.Add(new(Key: c.Key, GroupMembers: members));
            }
            else
            {
                if (c.EnumOption is not null)
                {
                    choices.Add(new(Key: c.Key, EnumOptionCode: c.EnumOption.Code));
                }
                else if (c.LookupOption is not null)
                {
                    choices.Add(new(Key: c.Key, LookupOptionCode: c.LookupOption.Code));
                }
                else
                {
                    choices.Add(new(Key: c.Key, NumericValue: c.NumericValue!.Value));
                }
            }
        }

        return new VariantAxisSnapshot(
            DefinitionKey: Definition.Key,
            IsGroup: IsGroup,
            Choices: choices);
    }

    internal static int Comparison(VariantAxis a, VariantAxis b)
    {
        var cmp = a.Definition.Position.CompareTo(b.Definition.Position);
        return cmp != 0
            ? cmp
            : string.Compare(a.Definition.Key, b.Definition.Key, StringComparison.Ordinal);
    }

    public override string ToString() => $"{Definition}×{Cardinality}";
}

/// <summary>
/// Atomic selection for a single attribute in a variant combination:
/// the attribute definition and its normalized (non-group) choice.
/// </summary>
/// <param name="Definition">The attribute definition associated with the selection.</param>
/// <param name="Choice">The normalized choice made for the attribute.</param>
public sealed record AxisSelection(
    AttributeDefinition Definition,
    NormalizedAxisChoice Choice);

/// <summary>
/// One "pick" contributed by a single axis during cartesian expansion.
/// For single axes this contains exactly one (definition, choice);
/// for group axes it contains one pair per group member (already flattened to normalized choices).
/// </summary>
public sealed class AxisPick : List<AxisSelection>
{
    public AxisPick() { }
    public AxisPick(int capacity) : base(capacity) { }
}
