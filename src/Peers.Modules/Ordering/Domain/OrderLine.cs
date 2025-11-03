using Peers.Modules.Listings.Domain;

namespace Peers.Modules.Ordering.Domain;

/// <summary>
/// Represents a single line item within an order, including the selected product variant, quantity, and pricing
/// information.
/// </summary>
public sealed class OrderLine : Entity
{
    /// <summary>
    /// The identifier of the order to which this line item belongs.
    /// </summary>
    public int OrderId { get; private set; }
    /// <summary>
    /// The identifier of the listing associated with this line item.
    /// </summary>
    public int ListingId { get; private set; }
    /// <summary>
    /// The identifier of the specific variant of the listing for this line item.
    /// </summary>
    public int VariantId { get; private set; }
    /// <summary>
    /// The quantity of the product variant in this line item.
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// The unit price captured at order time.
    /// </summary>
    public decimal UnitPrice { get; private set; }
    /// <summary>
    /// The total price for this line item (UnitPrice * Quantity).
    /// </summary>
    public decimal LineTotal => UnitPrice * Quantity;
    /// <summary>
    /// The order to which this line item belongs.
    /// </summary>
    public Order Order { get; private set; } = default!;
    /// <summary>
    /// The listing associated with this line item.
    /// </summary>
    public Listing Listing { get; set; } = default!;
    /// <summary>
    /// The specific variant of the listing for this line item.
    /// </summary>
    public ListingVariant Variant { get; set; } = default!;

    private OrderLine() { }

    /// <summary>
    /// Initializes a new instance of the OrderLine class with the specified listing variant and quantity.
    /// </summary>
    /// <param name="order">The order to which this order line belongs. Cannot be null.</param>
    /// <param name="variant">The listing variant associated with this order line. Cannot be null.</param>
    /// <param name="quantity">The number of units of the specified variant to include in the order. Must be greater than zero.</param>
    internal OrderLine(
        Order order,
        ListingVariant variant,
        int quantity)
    {
        Quantity = quantity;
        UnitPrice = variant.Price;
        Order = order;
        Variant = variant;
        Listing = variant.Listing;
    }
}
