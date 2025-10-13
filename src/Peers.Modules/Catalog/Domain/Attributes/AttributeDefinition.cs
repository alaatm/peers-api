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
[DebuggerDisplay("{D,nq}")]
public abstract class AttributeDefinition : Entity, ILocalizable<AttributeDefinition, AttributeDefinitionTr>
{
    /// <summary>
    /// The unique, non-localizable identifier (e.g., "color", "screen_size")
    /// </summary>
    public string Key { get; private set; } = default!;
    /// <summary>
    /// The data type associated with the attribute or its intended usage.
    /// </summary>
    public AttributeKind Kind { get; private set; }
    /// <summary>
    /// Indicates whether the associated item is required.
    /// </summary>
    public bool IsRequired { get; private set; }
    /// <summary>
    /// Indicates whether this attribute's value creates a unique, sellable variant of a listing.
    /// </summary>
    public bool IsVariant { get; private set; }
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

    /// <summary>
    /// Initializes a new instance of the AttributeDefinition class with the specified owner, key, kind, requirement,
    /// variant status, and position.
    /// </summary>
    /// <param name="owner">The product type that owns this attribute definition. Cannot be null.</param>
    /// <param name="key">The unique key identifying the attribute within the product type. Cannot be null or empty.</param>
    /// <param name="kind">The kind of attribute, indicating its data type or usage.</param>
    /// <param name="isRequired">A value indicating whether the attribute is required when creating a product or listing.</param>
    /// <param name="isVariant">Indicates whether this attribute's value creates a unique, sellable variant of a listing.</param>
    /// <param name="position">The position of the attribute; used for stable ordering.</param>
    protected AttributeDefinition(
        ProductType owner,
        string key,
        AttributeKind kind,
        bool isRequired,
        bool isVariant,
        int position)
    {
        ProductType = owner;
        Key = key;
        Kind = kind;
        IsRequired = isRequired;
        IsVariant = isVariant;
        Position = position;
        Translations = [];
    }

    internal virtual void Validate() { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public virtual string D => $"AD:{Id} - {Key} ({Kind}) | {(IsVariant ? "Variant" : "Non-variant")}";
}
