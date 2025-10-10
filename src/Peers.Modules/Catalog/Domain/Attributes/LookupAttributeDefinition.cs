using System.Globalization;
using Peers.Core.Domain.Errors;
using Peers.Modules.Lookup.Domain;
using E = Peers.Modules.Catalog.CatalogErrors;

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
    public LookupAttrConfig Config { get; set; }
    /// <summary>
    /// The associated lookup type.
    /// </summary>
    public LookupType LookupType { get; private set; } = default!;

    private LookupAttributeDefinition() : base() { }

    internal LookupAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        bool isVariant,
        int position,
        LookupType? lookupType) : base(owner, key, AttributeKind.Lookup, isRequired, isVariant, position)
    {
        ArgumentNullException.ThrowIfNull(lookupType);

        if (!lookupType.AllowVariant && isVariant)
        {
            throw new DomainException(E.LookupTypeDoesNotAllowVariants(lookupType.Key));
        }

        LookupType = lookupType;
        // TODO: Allow override
        Config = new LookupAttrConfig { ConstraintMode = lookupType.ConstraintMode };
    }

    internal override void Validate()
    {
        base.Validate();

        if (!LookupType.AllowVariant && IsVariant)
        {
            throw new DomainException(E.LookupTypeDoesNotAllowVariants(LookupType.Key));
        }
    }

    protected override string DebuggerDisplay
        => $"{base.DebuggerDisplay} | LookupType: {LookupType?.Key ?? LookupTypeId.ToString(CultureInfo.InvariantCulture)}";
}
