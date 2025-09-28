using System.Diagnostics;
using System.Text;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Logistics;
using E = Peers.Modules.Listings.ListingErrors;

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
    internal static ListingVariant CreateDefault(Listing listing) => new()
    {
        Listing = listing,
        VariantKey = "default",
        SkuCode = GenerateSku(listing.Id, []),
        Price = listing.BasePrice,
        IsActive = true,
        Attributes = [],
    };

    /// <summary>
    /// Creates a new ListingVariant instance for the specified listing and set of attribute options.
    /// </summary>
    /// <remarks>The axesOrdered parameter must be sorted by attribute position and then by key. The method
    /// generates a unique variant key and SKU code based on the provided axes.</remarks>
    /// <param name="listing">The listing to which the variant will belong. Cannot be null.</param>
    /// <param name="axesOrdered">A list of attribute definition and option pairs representing the variant's axes, ordered by attribute
    /// position and key.</param>
    internal static ListingVariant Create(
        Listing listing,
        IReadOnlyList<(AttributeDefinition def, EnumAttributeOption opt)> axesOrdered)
    {
        // Debug check: ensure caller sent axes sorted by Position then Key
        Debug.Assert(axesOrdered
            .OrderBy(x => x.def.Position).ThenBy(x => x.def.Key)
            .SequenceEqual(axesOrdered));

        var variantKey = string.Join("|", axesOrdered.Select(x => $"{x.def.Key}:{x.opt.Key}"));
        var skuCode = GenerateSku(listing.Id, axesOrdered);

        var v = new ListingVariant
        {
            Listing = listing,
            VariantKey = variantKey,
            SkuCode = skuCode,
            Price = listing.BasePrice,
            IsActive = true,
        };

        v.Attributes = [.. axesOrdered.Select(x => new ListingVariantAttribute(v, x.def, x.opt))];

        return v;
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

        if (!IsActive)
        {
            throw new DomainException(E.VariantInactive(SkuCode));
        }

        StockQty = quantity;
    }

    /// <summary>
    /// Updates the price of the variant to the specified value.
    /// </summary>
    /// <param name="price">The new price to assign to the variant.</param>
    internal void UpdatePrice(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price, nameof(price));

        if (!IsActive)
        {
            throw new DomainException(E.VariantInactive(SkuCode));
        }

        Price = price;
    }

    private static string GenerateSku(
        int listingId,
        IEnumerable<(AttributeDefinition def, EnumAttributeOption opt)> defOptList)
    {
        if (listingId <= 0)
        {
            throw new ArgumentException("Listing ID must be set.", nameof(listingId));
        }

        var tail = string.Join("-", defOptList.Select(x => Sanitize(x.opt.Key)));
        var prefix = listingId.EncodeBase36().ToUpperInvariant();
        var skuCode = tail.Length == 0
            ? $"SKU-{prefix}-DEFAULT"
            : $"SKU-{prefix}-{tail}";

        return skuCode;
    }

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
