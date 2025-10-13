namespace Peers.Modules.Listings.Domain.Snapshots;

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
