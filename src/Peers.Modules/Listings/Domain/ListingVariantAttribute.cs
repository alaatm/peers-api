using System.Diagnostics;
using System.Globalization;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Represents the association between a listing variant and a specific attribute option within that variant.
/// </summary>
[DebuggerDisplay("{D,nq}")]
public sealed partial class ListingVariantAttribute : IDebuggable
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
    /// The kind of the associated attribute definition.
    /// </summary>
    public AttributeKind AttributeKind { get; private set; }
    /// <summary>
    /// The identifier of the selected enum attribute option when this variant attribute is enum; otherwise, null.
    /// </summary>
    public int? EnumAttributeOptionId { get; private set; }
    /// <summary>
    /// The identifier of the selected lookup option when this variant attribute is lookup; otherwise, null.
    /// </summary>
    public int? LookupOptionId { get; private set; }
    /// <summary>
    /// The numeric payload when this variant attribute is numeric; otherwise null.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies only when <see cref="AttributeKind"/> is <c>Int</c> or <c>Decimal</c>. The value is stored in invariant
    /// units as defined by the attribute definition (do not include units here). For integer attributes, store the
    /// integral value (e.g., 150 as 150.0).
    /// </para>
    /// <para>
    /// This storage is per-member even when the numeric attribute participates in a composite
    /// <see cref="GroupAttributeDefinition"/> (e.g., “Size” composed of “Width” and “Length”); each member gets its own
    /// <see cref="ListingVariantAttribute"/> row with its numeric value.
    /// </para>
    /// <para><strong>Invariants:</strong> exactly one of <see cref="NumericValue"/>,
    /// <see cref="EnumAttributeOptionId"/>, or <see cref="LookupOptionId"/> is non-null, according to
    /// <see cref="AttributeKind"/>; <see cref="AttributeKind.Group"/> is not permitted here.</para>
    /// </remarks>
    public decimal? NumericValue { get; private set; }
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
    /// The selected enum attribute option when the variant attribute is enum; otherwise, null.
    /// </summary>
    public EnumAttributeOption? EnumAttributeOption { get; private set; } = default!;
    /// <summary>
    /// The selected lookup option when the variant attribute is lookup; otherwise, null.
    /// </summary>
    public LookupOption? LookupOption { get; private set; }

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
        EnumAttributeDefinition def,
        EnumAttributeOption option)
    {
        AttributeKind = def.Kind;
        ListingVariant = variant;
        AttributeDefinition = def;
        EnumAttributeOption = option;
        Position = def.Position;
    }

    internal ListingVariantAttribute(
        ListingVariant variant,
        LookupAttributeDefinition def,
        LookupOption lookupOption)
    {
        AttributeKind = def.Kind;
        ListingVariant = variant;
        AttributeDefinition = def;
        LookupOption = lookupOption;
        Position = def.Position;
    }

    internal ListingVariantAttribute(
        ListingVariant variant,
        NumericAttributeDefinition def,
        decimal numericValue)
    {
        AttributeKind = def.Kind;
        ListingVariant = variant;
        AttributeDefinition = def;
        NumericValue = numericValue;
        Position = def.Position;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"LVA:{ListingVariantId}:{AttributeDefinitionId} - {AttributeDefinition.D} = {NumericValue?.ToString(CultureInfo.InvariantCulture) ?? EnumAttributeOption?.D ?? LookupOption?.D ?? "<null>"}";
}
