using System.Globalization;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents the definition of an attribute whose value is selected from a global and shared predefined lookup type.
/// </summary>
/// <remarks>A lookup attribute definition specifies that the attribute's value must correspond to an entry in the
/// associated lookup type. This class is typically used to enforce consistency and restrict attribute values to a
/// controlled set defined by the lookup type.</remarks>
public sealed class LookupAttributeDefinition : DependentAttributeDefinition
{
    /// <summary>
    /// The identifier of the associated lookup type.
    /// </summary>
    public int LookupTypeId { get; private set; }
    /// <summary>
    /// The associated lookup type.
    /// </summary>
    public LookupType LookupType { get; private set; } = default!;

    private LookupAttributeDefinition() : base() { }

    internal LookupAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        int position,
        LookupType? lookupType) : base(owner, key, AttributeKind.Lookup, isRequired, position)
    {
        ArgumentNullException.ThrowIfNull(lookupType);
        LookupType = lookupType;
    }

    protected override string DebuggerDisplay
        => $"{base.DebuggerDisplay} | LookupType: {LookupType?.Key ?? LookupTypeId.ToString(CultureInfo.InvariantCulture)}";
}
