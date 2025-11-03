using Peers.Modules.Listings.Domain;

namespace Peers.Modules.Carts.Domain;

/// <summary>
/// Represents a single item in a shopping cart, including the selected product variant, quantity, and pricing
/// information.
/// </summary>
/// <remarks>A CartLine associates a specific product variant with a cart and tracks the quantity and unit price
/// at the time the item is added. The unit price is captured when the CartLine is created and does not automatically
/// update if the product price changes later. CartLine instances are typically managed by the cart and are not intended
/// to be created directly by consumers.</remarks>
public sealed class CartLine : Entity
{
    /// <summary>
    /// The identifier of the cart to which this line item belongs.
    /// </summary>
    public int CartId { get; private set; }
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
    /// The cart to which this line item belongs.
    /// </summary>
    public Cart Cart { get; private set; } = default!;
    /// <summary>
    /// The listing associated with this line item.
    /// </summary>
    public Listing Listing { get; set; } = default!;
    /// <summary>
    /// The specific variant of the listing for this line item.
    /// </summary>
    public ListingVariant Variant { get; set; } = default!;

    private CartLine() { }

    /// <summary>
    /// Initializes a new instance of the CartLine class with the specified cart, listing variant, and quantity.
    /// </summary>
    /// <param name="cart">The cart to which this cart line belongs. Cannot be null.</param>
    /// <param name="variant">The listing variant associated with this cart line. Cannot be null.</param>
    /// <param name="quantity">The number of units of the specified variant to add to the cart. Must be greater than zero.</param>
    internal CartLine(
        Cart cart,
        ListingVariant variant,
        int quantity)
    {
        Quantity = quantity;
        UnitPrice = variant.Price;
        Cart = cart;
        Variant = variant;
        Listing = variant.Listing;
    }
}
