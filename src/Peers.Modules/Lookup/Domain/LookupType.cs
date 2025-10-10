using System.Diagnostics;

namespace Peers.Modules.Lookup.Domain;

/// <summary>
/// Logical catalog for a family of lookup types (e.g., "brand", "device_model").
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
    /// The default constraint behavior applied when this lookup type is used
    /// by a <see cref="Catalog.Domain.Attributes.LookupAttributeDefinition"/> and no explicit override is set.
    /// </summary>
    /// <remarks>
    /// - If <see cref="LookupConstraintMode.Open"/>, listings may use any option of this lookup type
    ///   unless a product type (or its nearest ancestor) declares an allow-list.
    /// - If <see cref="LookupConstraintMode.RequireAllowList"/>, a product type must provide an
    ///   allow-list in its lineage for options of this type to be considered valid.
    /// </remarks>
    public LookupConstraintMode ConstraintMode { get; set; }
    /// <summary>
    /// Indicates whether variant-level attributes of this type are allowed.
    /// </summary>
    public bool AllowVariant { get; set; }
    /// <summary>
    /// The list of lookup options associated with this type.
    /// </summary>
    public List<LookupOption> Options { get; set; } = default!;

    private string DebuggerDisplay => $"{Key}";
}
