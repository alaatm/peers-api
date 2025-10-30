using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Peers.Modules.Listings.Domain.Snapshots;

/// <summary>
/// Points to a single axis + the selected choice on that axis (by keys).
/// </summary>
/// <param name="DefinitionKey">The key of the attribute definition for the axis.</param>
/// <param name="ChoiceKey">The key of the selected choice within the axis.</param>
[DebuggerDisplay("{D,nq}")]
public sealed record AxisSelectionRef(
    string DefinitionKey,
    string ChoiceKey)
{
    [JsonIgnore]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"ASRef - {DefinitionKey} | {ChoiceKey}";
}
