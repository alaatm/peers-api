using Peers.Modules.Listings.Domain;

namespace Peers.Modules.Carts.Domain;

/// <summary>
/// Represents a single item in a checkout session, including the selected product variant, quantity, and pricing
/// information.
/// </summary>
/// <remarks>A CheckoutSessionLine associates a specific product variant with a checkout session and tracks the quantity and unit price
/// at the time the item is added. The unit price is captured when the CheckoutSessionLine is created and does not automatically
/// update if the product price changes later. CheckoutSessionLine instances are typically managed by the checkout session and are not intended
/// to be created directly by consumers.</remarks>
public sealed class CheckoutSessionLine
{
    /// <summary>
    /// The identifier of the session to which this line item belongs.
    /// </summary>
    public int SessionId { get; private set; }
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
    /// The session to which this line item belongs.
    /// </summary>
    public CheckoutSession Session { get; private set; } = default!;
    /// <summary>
    /// The listing associated with this line item.
    /// </summary>
    public Listing Listing { get; set; } = default!;
    /// <summary>
    /// The specific variant of the listing for this line item.
    /// </summary>
    public ListingVariant Variant { get; set; } = default!;

    /// <summary>
    /// Returns the total price for this line item, calculated as UnitPrice multiplied by Quantity.
    /// </summary>
    public decimal LineTotal => UnitPrice * Quantity;

    private CheckoutSessionLine() { }

    /// <summary>
    /// Initializes a new instance of the CheckoutSessionLine class with the specified session, listing variant, and quantity.
    /// </summary>
    /// <param name="session">The session to which this line item belongs. Cannot be null.</param>
    /// <param name="variant">The listing variant associated with this line item. Cannot be null.</param>
    /// <param name="quantity">The number of units of the specified variant to add to the session. Must be greater than zero.</param>
    internal CheckoutSessionLine(
        CheckoutSession session,
        ListingVariant variant,
        int quantity)
    {
        Quantity = quantity;
        UnitPrice = variant.Price;
        Session = session;
        Variant = variant;
        Listing = variant.Listing;
    }
}
