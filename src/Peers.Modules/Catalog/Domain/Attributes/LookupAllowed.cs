using System.Diagnostics;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents per lookup attribute allowed option, used to curate picker options (e.g., allowed brands).
/// </summary>
[DebuggerDisplay("{D,nq}")]
public sealed class LookupAllowed
{
    /// <summary>
    /// The identifier of the associated attribute definition.
    /// </summary>
    public int AttributeDefinitionId { get; set; }
    /// <summary>
    /// The identifier of the lookup type being curated.
    /// </summary>
    public int TypeId { get; set; }
    /// <summary>
    /// The identifier of the allowed lookup option within <see cref="TypeId"/>
    /// </summary>
    public int OptionId { get; set; }
    /// <summary>
    /// The owning lookup attribute definition.
    /// </summary>
    public LookupAttributeDefinition AttributeDefinition { get; set; } = default!;
    /// <summary>
    /// The allowed lookup option.
    /// </summary>
    public LookupOption Option { get; set; } = default!;

    private LookupAllowed() { }

    /// <summary>
    /// Initializes a new instance of the LookupAllowed class with the specified attribute definition and lookup option.
    /// </summary>
    /// <param name="attributeDefinition">The attribute definition that determines the criteria for the lookup operation.</param>
    /// <param name="option">The lookup option that specifies how the lookup should be performed.</param>
    internal LookupAllowed(
        LookupAttributeDefinition attributeDefinition,
        LookupOption option)
    {
        AttributeDefinition = attributeDefinition;
        Option = option;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"{Option.D}";
}
