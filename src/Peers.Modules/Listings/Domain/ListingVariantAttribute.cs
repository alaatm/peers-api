using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Represents the association between a listing variant and a specific attribute option within that variant.
/// </summary>
public sealed class ListingVariantAttribute
{
    /// <summary>
    /// The identifier of the listing variant this attribute belongs to.
    /// </summary>
    public int ListingVariantId { get; private set; }
    /// <summary>
    /// The identifier of the attribute definition this attribute is based on.
    /// </summary>
    public int AttributeDefinitionId { get; private set; }
    /// <summary>
    /// The identifier of the selected enum attribute option for this variant attribute.
    /// </summary>
    public int AttributeOptionId { get; private set; }
    /// <summary>
    /// The position of the attribute in the product type's attribute list, used for ordering.
    /// </summary>
    public int Position { get; private set; }
    /// <summary>
    /// The listing variant this attribute belongs to.
    /// </summary>
    public ListingVariant ListingVariant { get; private set; } = default!;
    /// <summary>
    /// The attribute definition this variant attribute is based on.
    /// </summary>
    public AttributeDefinition AttributeDefinition { get; private set; } = default!;
    /// <summary>
    /// The selected enum attribute option for this variant attribute.
    /// </summary>
    public EnumAttributeOption EnumAttributeOption { get; private set; } = default!;

    private ListingVariantAttribute() { }

    /// <summary>
    /// Initializes a new instance of the ListingVariantAttribute class with the specified variant, attribute
    /// definition, and enumeration option.
    /// </summary>
    /// <param name="variant">The listing variant to which this attribute belongs.</param>
    /// <param name="def">The definition of the attribute to associate with the variant.</param>
    /// <param name="option">The enumeration option selected for this attribute.</param>
    internal ListingVariantAttribute(
        ListingVariant variant,
        AttributeDefinition def,
        EnumAttributeOption option)
    {
        ListingVariant = variant;
        AttributeDefinition = def;
        EnumAttributeOption = option;
        Position = def.Position;
    }
}
