using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Canonical, serialized set of variant axes for a listing.
/// </summary>
/// <param name="SnapshotId">A stable identifier for this snapshot.</param>
/// <param name="Version">Version of the snapshot structure; increment when you regenerate axes snapshot structure.</param>
/// <param name="CreatedAt">Timestamp when the snapshot was created.</param>
/// <param name="Axes">The list of variant axes and their offered choices, in canonical order.</param>
public sealed record VariantAxesSnapshot(
    string SnapshotId,
    int Version,
    DateTimeOffset CreatedAt,
    List<VariantAxisSnapshot> Axes)
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
}

/// <summary>
/// A single axis (definition) and the offered choices in canonical order.
/// </summary>
/// <param name="DefinitionKey">The key of the attribute definition for this axis (e.g., "color", "size").</param>
/// <param name="IsGroup">True if this is a composite/group axis; false for single axes.</param>
/// <param name="Choices">The list of offered choices for this axis, in canonical order.</param>
public sealed record VariantAxisSnapshot(
    string DefinitionKey,
    bool IsGroup,
    List<AxisChoiceSnapshot> Choices)
{
    internal VariantAxis ToRuntime(ProductType productType)
    {
        var choices = new List<AxisChoice>(Choices.Count);
        var def = productType.Attributes.Single(p => p.Key == DefinitionKey);

        foreach (var c in Choices)
        {
            if (c.GroupMembers is not null)
            {
                var groupDef = (GroupAttributeDefinition)def;
                var members = new List<AxisChoice.GroupMember>(c.GroupMembers.Count);

                foreach (var member in c.GroupMembers)
                {
                    var memberDef = groupDef.Members.Single(p => p.Key == member.MemberDefinitionKey);
                    members.Add(new(memberDef, member.Value));
                }

                choices.Add(new AxisChoice(Key: c.Key, GroupMembers: members));
            }
            else
            {
                if (c.EnumOptionCode is not null)
                {
                    var enumDef = (EnumAttributeDefinition)def;
                    var option = enumDef.Options.Single(p => p.Code == c.EnumOptionCode);
                    choices.Add(new AxisChoice(Key: c.Key, EnumOption: option));
                }
                else if (c.LookupOptionCode is not null)
                {
                    var lookupDef = (LookupAttributeDefinition)def;
                    var option = lookupDef.LookupType.Options.Single(p => p.Code == c.LookupOptionCode);
                    choices.Add(new AxisChoice(Key: c.Key, LookupOption: option));
                }
                else if (c.NumericValue is not null)
                {
                    var numericDef = (NumericAttributeDefinition)def;
                    choices.Add(new AxisChoice(Key: c.Key, NumericValue: c.NumericValue.Value));
                }
                else
                {
                    throw new InvalidOperationException("Invalid AxisChoiceSnapshot: no matching runtime representation found.");
                }
            }
        }

        return new(def, choices);
    }
}

/// <summary>
/// One offered choice on an axis. Exactly 1 of (enum/lookup/numeric/groupMembers) is set.
/// </summary>
/// <param name="Key">
/// A stable, unique key within the axis for this choice within the axis.
/// <list type="bullet">
///     <item>
///         <term>Enum</term>
///         <description>The option code. (e.g., "red", "large").</description>
///     </item>
///     <item>
///         <term>Lookup</term>
///         <description>The option code. (e.g., "us", "ca").</description>
///     </item>
///     <item>
///         <term>Numeric</term>
///         <description>The numeric value as a string. (e.g., "150").</description>
///     </item>
///     <item>
///         <term>Group</term>
///         <description>Canonical "memberKey=value" pairs joined by commas (ordered by member Position→Key), e.g., "width=150,length=400".</description>
///     </item>
/// </list>
/// </param>
/// <param name="EnumOptionCode">The enum option code; null if not using an enum.</param>
/// <param name="LookupOptionCode">The lookup option code; null if not using a lookup.</param>
/// <param name="NumericValue">The numeric value; null if not using a numeric.</param>
/// <param name="GroupMembers">A list of group members representing the axis value; null if not using a group.</param>
public sealed record AxisChoiceSnapshot(
    string Key,
    string? EnumOptionCode = null,
    string? LookupOptionCode = null,
    decimal? NumericValue = null,
    List<AxisChoiceSnapshot.GroupMemberSnapshot>? GroupMembers = null)
{
    /// <summary>
    /// One member of a group tuple: member definition key + numeric value.
    /// </summary>
    /// <param name="MemberDefinitionKey">The key of the member attribute definition (e.g., "width", "length").</param>
    /// <param name="Value">The numeric value for this group member.</param>
    public sealed record GroupMemberSnapshot(string MemberDefinitionKey, decimal Value);
}

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

/// <summary>
/// Points to a single axis + the selected choice on that axis (by keys).
/// </summary>
/// <param name="DefinitionKey">The key of the attribute definition for the axis.</param>
/// <param name="ChoiceKey">The key of the selected choice within the axis.</param>
public sealed record AxisSelectionRef(
    string DefinitionKey,
    string ChoiceKey);
