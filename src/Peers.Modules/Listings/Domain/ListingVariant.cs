using System.Diagnostics;
using System.Text;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Listings.Domain.Snapshots;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Represents a specific variant of a product listing, defined by a unique combination of attribute options such as
/// color or size.
/// </summary>
/// <remarks>A listing variant encapsulates variant-specific details including SKU, price override, inventory
/// quantity, and activation status. Each variant is associated with its parent listing and a set of attributes that
/// describe its distinguishing features. Variants are typically used to manage inventory and pricing for products that
/// come in multiple forms (for example, a shirt available in different sizes and colors).</remarks>
public sealed class ListingVariant : Entity
{
    public const string DefaultVariantKey = "default";

    /// <summary>
    /// The identifier of the listing this variant belongs to.
    /// </summary>
    public int ListingId { get; private set; }
    /// <summary>
    /// The unique key representing the combination of attribute options that define this variant.
    /// e.g. "color:red|size:m"
    /// </summary>
    public string VariantKey { get; private set; } = default!;
    /// <summary>
    /// The stock keeping unit (SKU) code for this variant, used for inventory management and identification.
    /// </summary>
    public string SkuCode { get; private set; } = default!;
    /// <summary>
    /// The price override for this variant. On creation, this is set to the listing's base price.
    /// It can be updated by the seller to a different value if desired.
    /// </summary>
    public decimal Price { get; private set; }
    /// <summary>
    /// The inventory quantity available for this variant. A null value indicates unlimited inventory.
    /// </summary>
    public int? StockQty { get; private set; }
    /// <summary>
    /// The activation status of this variant. If true, the variant is available for purchase; if false, it is inactive.
    /// </summary>
    public bool IsActive { get; private set; }
    /// <summary>
    /// The listing this variant belongs to.
    /// </summary>
    public Listing Listing { get; private set; } = default!;
    /// <summary>
    /// A snapshot of the variant selection at the time of creation, capturing the selected attribute options.
    /// This references <see cref="Listing.AxesSnapshot" /> plus the choices this variant selected.
    /// </summary>
    public VariantSelectionSnapshot SelectionSnapshot { get; private set; } = default!;
    /// <summary>
    /// The logistics profile associated with this variant, if any. This includes details such as dimensions, weight, etc.
    /// </summary>
    /// <remarks>
    /// This is required only when the listing fulfilment method is set to <see cref="FulfillmentMethod.PlatformManaged"/> and the
    /// product type kind is <see cref="ProductTypeKind.Physical"/>.
    /// </remarks>
    public LogisticsProfile? Logistics { get; private set; }
    /// <summary>
    /// The list of attributes that define this variant, each representing a specific attribute option
    /// </summary>
    public List<ListingVariantAttribute> Attributes { get; private set; } = default!;

    private ListingVariant() { }

    /// <summary>
    /// Creates a default variant for the specified listing. This is used when a listing has no variant axes.
    /// </summary>
    /// <param name="listing">The listing for which to create the default variant.</param>
    /// <param name="selectionSnapshot">A snapshot referencing the listing's axes snapshot with no selections.</param>
    internal static ListingVariant CreateDefault(
        Listing listing,
        VariantSelectionSnapshot selectionSnapshot) => new()
        {
            Listing = listing,
            SelectionSnapshot = selectionSnapshot,
            VariantKey = DefaultVariantKey,
            SkuCode = GenerateSku(listing.Id, null),
            Price = listing.BasePrice,
            IsActive = true,
            Attributes = [],
        };

    /// <summary>
    /// Creates a new <see cref="ListingVariant"/> and its per-SKU attribute selections from the given axes.
    /// </summary>
    /// <param name="listing">The owning listing (non-null).</param>
    /// <param name="axis">
    /// One entry per variant axis (never a Group), ordered by the attribute definition's <c>Position</c> then <c>Key</c>.
    /// For each entry, set exactly one value matching the definition type:
    /// enum → <c>enumOption</c>; lookup → <c>lookupOption</c>; numeric (int/decimal) → <c>numericValue</c>.
    /// </param>
    /// <param name="selectionSnapshot">A snapshot referencing the listing's axes snapshot plus the choices this variant selected.</param>
    /// <remarks>
    /// Builds a canonical <c>VariantKey</c> from ordered <c>def.Key:value</c> tokens and a SKU code from the same tokens.
    /// Numeric tokens use invariant formatting (Int without decimals; Decimal as <c>G29</c>).
    /// </remarks>
    internal static ListingVariant Create(
        Listing listing,
        AxisPick axis,
        VariantSelectionSnapshot selectionSnapshot)
    {

        var choiceSegments = axis.Select(p => $"{p.Definition.Key}:{GetAxisChoiceValue(p.Choice)}");
        var variantKey = string.Join('|', choiceSegments);
        var skuCode = GenerateSku(listing.Id, axis);

        var v = new ListingVariant
        {
            Listing = listing,
            SelectionSnapshot = selectionSnapshot,
            VariantKey = variantKey,
            SkuCode = skuCode,
            Price = listing.BasePrice,
            IsActive = false,
        };

        v.Attributes = [.. axis.Select(p =>
            p.Definition switch
            {
                EnumAttributeDefinition e => new ListingVariantAttribute(v, e, p.Choice.EnumOption!),
                LookupAttributeDefinition l => new ListingVariantAttribute(v, l, p.Choice.LookupOption!),
                NumericAttributeDefinition n => new ListingVariantAttribute(v, n, p.Choice.NumericValue!.Value),
                _ => throw new UnreachableException(),
            }
        )];

        return v;
    }

    private static string GenerateSku(
        int listingId,
        AxisPick? axis)
    {
        if (listingId <= 0)
        {
            throw new ArgumentException("Listing ID must be set.", nameof(listingId));
        }

        var choiceSegments = axis is not null
            ? axis.Select(p => Sanitize(GetAxisChoiceValue(p.Choice)))
            : [];
        var tail = string.Join('-', choiceSegments);
        var prefix = listingId.EncodeBase36().ToUpperInvariant();
        var skuCode = tail.Length == 0
            ? $"SKU-{prefix}-DEFAULT"
            : $"SKU-{prefix}-{tail}";

        return skuCode;
    }


    /// <summary>
    /// Determines whether the specified quantity of stock is available for purchase.
    /// </summary>
    /// <remarks>If the inventory quantity is not set, the method assumes unlimited stock is
    /// available.</remarks>
    /// <param name="quantity">The number of units requested. Must be a non-negative value.</param>
    /// <returns>true if the available inventory is greater than or equal to the specified quantity, or if the inventory quantity
    /// is unspecified; otherwise, false.</returns>
    public bool HasStockAvailable(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        if (!StockQty.HasValue)
        {
            return true;
        }

        return IsActive && StockQty.Value >= quantity;
    }

    /// <summary>
    /// Sets the logistics profile for the current instance.
    /// </summary>
    /// <param name="logistics">The logistics profile to assign.</param>
    internal void SetLogistics(LogisticsProfile logistics)
        => Logistics = logistics;

    /// <summary>
    /// Clears the current logistics information.
    /// </summary>
    internal void ClearLogistics()
        => Logistics = null;

    /// <summary>
    /// Updates the stock quantity for the current variant.
    /// </summary>
    /// <param name="quantity">The new stock quantity to set for the variant.</param>
    internal void UpdateStockQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity, nameof(quantity));
        StockQty = quantity;
    }

    /// <summary>
    /// Updates the price of the variant to the specified value.
    /// </summary>
    /// <param name="price">The new price to assign to the variant.</param>
    internal void UpdatePrice(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price, nameof(price));
        Price = price;
    }

    internal void Validate()
    {
        Logistics?.Validate();
    }

    private static string GetAxisChoiceValue(NormalizedAxisChoice choice)
        => choice.EnumOption?.Code ?? choice.LookupOption?.Code ?? choice.NumericValue!.Value.Normalize();

    private static string Sanitize(string code)
    {
        var sb = new StringBuilder(code.Length);
        foreach (var ch in code)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToUpperInvariant(ch));
            }
            else if (ch is '-' or '_' or ' ')
            {
                sb.Append('-');
            }
        }

        return sb.ToString().Trim('-');
    }
}
