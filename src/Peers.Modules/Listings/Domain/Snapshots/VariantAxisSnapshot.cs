using System.Diagnostics;
using System.Text.Json.Serialization;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Listings.Domain.Snapshots;

/// <summary>
/// A single axis (definition) and the offered choices in canonical order.
/// </summary>
/// <param name="DefinitionKey">The key of the attribute definition for this axis (e.g., "color", "size").</param>
/// <param name="IsGroup">True if this is a composite/group axis; false for single axes.</param>
/// <param name="Choices">The list of offered choices for this axis, in canonical order.</param>
[DebuggerDisplay("{D,nq}")]
public sealed partial record VariantAxisSnapshot(
    string DefinitionKey,
    bool IsGroup,
    List<AxisChoiceSnapshot> Choices) : IDebuggable
{
    internal VariantAxis ToRuntime(ProductType productType)
    {
        var choices = new List<AxisChoice>(Choices.Count);
        var def = productType.Attributes.Find(p => p.Key == DefinitionKey)!;

        foreach (var c in Choices)
        {
            if (c.GroupMembers is not null)
            {
                var groupDef = (GroupAttributeDefinition)def;
                var members = new List<AxisChoice.GroupMember>(c.GroupMembers.Count);

                foreach (var member in c.GroupMembers)
                {
                    var memberDef = groupDef.Members.Find(p => p.Key == member.MemberDefinitionKey)
                        ?? throw new InvalidDomainStateException($"Group member definition '{member.MemberDefinitionKey}' not found in runtime representation.");
                    members.Add(new(memberDef, member.Value));
                }

                choices.Add(new AxisChoice(Key: c.Key, GroupMembers: members));
            }
            else
            {
                if (c.EnumOptionCode is not null)
                {
                    var enumDef = (EnumAttributeDefinition)def;
                    var option = enumDef.Options.Find(p => p.Code == c.EnumOptionCode)
                        ?? throw new InvalidDomainStateException($"Enum option '{c.EnumOptionCode}' not found in runtime representation.");
                    choices.Add(new AxisChoice(Key: c.Key, EnumOption: option));
                }
                else if (c.LookupOptionCode is not null)
                {
                    var lookupDef = (LookupAttributeDefinition)def;
                    var option = lookupDef.LookupType.Options.Find(p => p.Code == c.LookupOptionCode)
                        ?? throw new InvalidDomainStateException($"Lookup option '{c.LookupOptionCode}' not found in runtime representation.");
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

    [JsonIgnore]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"VAxisSnap - {DefinitionKey} ({(IsGroup ? "Group" : "Individual")}) | {Choices.Count} choices";
}
