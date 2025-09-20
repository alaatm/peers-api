using System.Diagnostics;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents the definition of an attribute, including its metadata, constraints, and relationships.
/// </summary>
/// <remarks>
/// This class is used to define the characteristics of an attribute, such as its key, label, data type, 
/// and constraints. It also supports relationships to other attributes and product types, enabling  complex attribute
/// configurations. Instances of this class are immutable after creation.
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class AttributeDefinition : Entity, ILocalizable<AttributeDefinition, AttributeDefinitionTr>
{
    /// <summary>
    /// The unique, non-localizable identifier (e.g., "color", "screen_size")
    /// </summary>
    public string Key { get; private set; } = default!;
    /// <summary>
    /// The data type associated with the attribute.
    /// </summary>
    public AttributeKind Kind { get; private set; }
    /// <summary>
    /// Indicates whether the associated item is required.
    /// </summary>
    public bool IsRequired { get; private set; }
    /// <summary>
    /// The position of the attribute; used for stable ordering.
    /// </summary>
    public int Position { get; private set; }
    /// <summary>
    /// The identifier of the associated product type.
    /// </summary>
    public int ProductTypeId { get; private set; }
    /// <summary>
    /// The associated product type.
    /// </summary>
    public ProductType ProductType { get; private set; } = default!;
    /// <summary>
    /// The list of translations associated with this attribute definition.
    /// </summary>
    public List<AttributeDefinitionTr> Translations { get; private set; } = default!;

    protected AttributeDefinition() { }

    protected AttributeDefinition(
        ProductType owner,
        string key,
        AttributeKind kind,
        bool isRequired,
        int position)
    {
        ProductType = owner;
        Key = key;
        Kind = kind;
        IsRequired = isRequired;
        Position = position;
        Translations = [];
    }

    protected virtual string DebuggerDisplay => $"{Key} ({Kind})";
}
