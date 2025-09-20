using System.Diagnostics;

namespace Peers.Modules.Lookup.Domain;

/// <summary>
/// Logical catalog for a family of lookup values (e.g., "brand", "device_model").
/// </summary>
/// <remarks>
/// - Keys are global and stable; one <see cref="LookupType"/> is reused across many product types.
/// - Referenced by <see cref="Catalog.Domain.Attributes.LookupAttributeDefinition"/>.
/// - Uniqueness: <c>Key</c> is unique; values under this type enforce their own (TypeId, Key) uniqueness.
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class LookupType : Entity, IAggregateRoot
{
    /// <summary>
    /// Stable ASCII key/slug for this type (e.g., "brand", "model").
    /// </summary>
    public string Key { get; set; } = default!;
    /// <summary>
    /// The list of lookup values associated with this type.
    /// </summary>
    public List<LookupValue> Values { get; set; } = default!;

    private string DebuggerDisplay => $"{Key}";
}
