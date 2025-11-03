using Peers.Core.Domain.Errors;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Ordering.Domain;
using E = Peers.Modules.Carts.CartsErrors;

namespace Peers.Modules.Carts.Domain;

/// <summary>
/// Represents a shopping cart containing line items selected by a buyer from a specific seller.
/// </summary>
/// <remarks>A Cart tracks the items a buyer intends to purchase from a seller, along with associated metadata
/// such as creation and update timestamps. It enforces business rules such as ensuring that only published listings
/// from the correct seller can be added, and that line item quantities are valid. The Cart supports operations for
/// adding, removing, and updating line items, as well as restoring its state from an order and attempting checkout.
/// After a successful checkout, the cart is cleared. This type is immutable with respect to its identity and
/// participants after creation.
/// 
/// For each (buyer, seller) pair, there is at most one cart at any time.
/// </remarks>
public sealed class Cart : Entity, IAggregateRoot
{
    /// <summary>
    /// The unique identity of the buyer.
    /// </summary>
    public int BuyerId { get; private set; }
    /// <summary>
    /// The unique identity of the seller.
    /// </summary>
    public int SellerId { get; private set; }
    /// <summary>
    /// The creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// The last updated timestamp.
    /// </summary>
    public DateTime LastTouchedAt { get; private set; }
    /// <summary>
    /// The customer who placed the order.
    /// </summary>
    public Customer Buyer { get; private set; } = default!;
    /// <summary>
    /// The customer who is selling the items.
    /// </summary>
    public Customer Seller { get; private set; } = default!;
    /// <summary>
    /// The list of line items in the cart.
    /// </summary>
    public List<CartLine> Lines { get; private set; } = default!;

    private Cart() { }

    /// <summary>
    /// Creates a new cart instance for the specified buyer and seller with the given creation date.
    /// </summary>
    /// <param name="buyer">The customer who is purchasing items. Cannot be the same as <paramref name="seller"/>.</param>
    /// <param name="seller">The customer who is selling items. Cannot be the same as <paramref name="buyer"/>.</param>
    /// <param name="date">The date and time when the cart is created. Sets both the creation and last touched timestamps.</param>
    /// <returns>A new <see cref="Cart"/> initialized with the specified buyer, seller, and creation date.</returns>
    /// <exception cref="DomainException">Thrown if <paramref name="buyer"/> and <paramref name="seller"/> refer to the same customer.</exception>
    public static Cart Create(
        Customer buyer,
        Customer seller,
        DateTime date)
    {
        if (buyer == seller)
        {
            throw new DomainException(E.BuyerSellerSame);
        }

        return new()
        {
            CreatedAt = date,
            LastTouchedAt = date,
            Buyer = buyer,
            Seller = seller,
            Lines = [],
        };
    }

    /// <summary>
    /// Adds a line item to the cart for the specified listing variant and quantity, or updates the quantity if the line
    /// item already exists.
    /// </summary>
    /// <remarks>If a line item for the specified variant already exists in the cart, its quantity is
    /// increased by the specified amount. Otherwise, a new line item is added.</remarks>
    /// <param name="listing">The listing to add to the cart. The listing must be published and its seller must match the cart's seller.</param>
    /// <param name="variantKey">The key identifying the specific variant of the listing to add.</param>
    /// <param name="quantity">The number of units to add. Must be zero or greater.</param>
    /// <param name="date">The date and time to record as the last modification to the cart.</param>
    /// <returns>The cart line representing the added or updated line item.</returns>
    /// <exception cref="DomainException">Thrown if the listing is not published, the listing's seller does not match the cart's seller, or the specified
    /// variant does not exist in the listing.</exception>
    public CartLine AddLineItem(
        [NotNull] Listing listing,
        string variantKey,
        int quantity,
        DateTime date)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        if (listing.State is not ListingState.Published)
        {
            throw new DomainException(E.ListingNotPublished);
        }

        if (listing.Seller != Seller)
        {
            throw new DomainException(E.ListingSellerMismatch);
        }

        if (listing.Variants.Find(v => v.VariantKey == variantKey) is not { } variant)
        {
            throw new DomainException(E.VariantNotFound(variantKey));
        }

        // If the line already exists, we are updating the quantity.

        if (Lines.Find(l => l.Variant == variant) is { } existingLine)
        {
            UpdateLineItemQuantity(existingLine, existingLine.Quantity + quantity, date);
            return existingLine;
        }

        ValidateLine(variant, quantity);

        var line = new CartLine(this, variant, quantity);
        Lines.Add(line);

        LastTouchedAt = date;
        return line;
    }

    /// <summary>
    /// Removes a line item from the cart that matches the specified listing and variant key.
    /// </summary>
    /// <param name="listing">The listing associated with the line item to remove. Cannot be null.</param>
    /// <param name="variantKey">The unique key identifying the variant of the listing to remove.</param>
    /// <param name="date">The date associated with the removal operation.</param>
    public void RemoveLineItem(
        [NotNull] Listing listing,
        string variantKey,
        DateTime date)
    {
        var cartLine = Lines.Find(p =>
            p.Variant.Listing == listing &&
            p.Variant.VariantKey == variantKey);

        RemoveLineItem(cartLine, date);
    }

    /// <summary>
    /// Updates the quantity of a line item in the cart that matches the specified listing and variant key.
    /// </summary>
    /// <param name="listing">The listing associated with the line item to update. Cannot be null.</param>
    /// <param name="variantKey">The unique key identifying the variant of the listing to update.</param>
    /// <param name="newQuantity">The new quantity to set for the specified line item. Must be zero or greater.</param>
    /// <param name="date">The date and time when the quantity update is applied.</param>
    public void UpdateLineItemQuantity(
        [NotNull] Listing listing,
        string variantKey,
        int newQuantity,
        DateTime date)
    {
        var cartLine = Lines.Find(p =>
            p.Variant.Listing == listing &&
            p.Variant.VariantKey == variantKey);

        UpdateLineItemQuantity(cartLine, newQuantity, date);
    }

    /// <summary>
    /// Restores the cart to match the state of the specified order.
    /// </summary>
    /// <remarks>This method does not validate stock when restoring the cart. Stock validation will occur
    /// again at checkout. This method is used to revert the cart to its state before a checkout operation took place.</remarks>
    /// <param name="order">The order whose state is used to restore the cart. Must have the same buyer and seller as the cart and be in the
    /// Placed state.</param>
    /// <param name="date">The date and time when the restore operation is requested.</param>
    /// <exception cref="DomainException">Thrown if the cart is not empty, if the order's buyer or seller does not match the cart, or if the order is not
    /// in the Placed state.</exception>
    public void Restore(
        [NotNull] Order order,
        DateTime date)
    {
        if (Lines.Count != 0)
        {
            throw new DomainException(E.CartNotEmpty);
        }

        if (order.Buyer != Buyer || order.Seller != Seller)
        {
            throw new DomainException(E.CustomerMismatch);
        }

        if (order.State is not OrderState.Placed)
        {
            throw new DomainException(E.OrderNotInPlacedState);
        }

        // Do not validate stock here, just restore the cart as it was at checkout.
        // We will validate stock again at checkout.
        foreach (var orderLine in order.Lines)
        {
            Lines.Add(new CartLine(this, orderLine.Variant, orderLine.Quantity));
        }

        LastTouchedAt = date;
    }

    /// <summary>
    /// Attempts to create an order from the current cart lines, validating each item before checkout.
    /// </summary>
    /// <remarks>If any cart line fails validation, the method does not create an order and provides error
    /// details for each invalid line. On successful checkout, the cart is cleared and the provided date is recorded as
    /// the last modification time.</remarks>
    /// <param name="date">The date and time to associate with the checkout operation. Typically represents when the checkout is performed.</param>
    /// <param name="order">When this method returns true, contains the created <see cref="Order"/>; otherwise, false.</param>
    /// <param name="errors">When this method returns false, contains a dictionary mapping invalid cart lines to error
    /// codes; otherwise, null.</param>
    /// <returns>true if the checkout succeeds and an order is created; otherwise, false.</returns>
    public bool TryCheckout(
        DateTime date,
        [NotNullWhen(true)] out Order? order,
        [NotNullWhen(false)] out IReadOnlyDictionary<CartLine, string>? errors)
    {
        order = null;
        var localErrors = new Dictionary<CartLine, string>();

        foreach (var line in Lines)
        {
            try
            {
                ValidateLine(line.Variant, line.Quantity);
            }
            catch (DomainException ex)
            {
                localErrors[line] = ex.Error.Code;
            }
        }

        if (localErrors.Count > 0)
        {
            errors = localErrors;
            return false;
        }

        errors = null;
        order = Order.Create(this);

        Lines.Clear();
        LastTouchedAt = date;

        return true;
    }

    private void RemoveLineItem(CartLine? line, DateTime date)
    {
        if (line is null || !Lines.Remove(line))
        {
            throw new DomainException(E.LineNotFound);
        }

        LastTouchedAt = date;
    }

    private void UpdateLineItemQuantity(CartLine? line, int newQuantity, DateTime date)
    {
        if (line is null || !Lines.Contains(line))
        {
            throw new DomainException(E.LineNotFound);
        }

        ArgumentOutOfRangeException.ThrowIfNegative(newQuantity);

        if (newQuantity == 0)
        {
            RemoveLineItem(line, date);
        }
        else if (newQuantity != line.Quantity)
        {
            ValidateLine(line.Variant, newQuantity);
            line.Quantity = newQuantity;
            LastTouchedAt = date;
        }
    }

    private static void ValidateLine(ListingVariant variant, int quantity)
    {
        var listing = variant.Listing;

        if (listing.State is not ListingState.Published)
        {
            throw new DomainException(E.ListingNotPublished);
        }

        if (listing.FulfillmentPreferences.OrderQtyPolicy is { } orderQtyPolicy &&
            !orderQtyPolicy.IsSatisfiedBy(quantity))
        {
            throw new DomainException(E.QtyOutOfRange(quantity, orderQtyPolicy.Min, orderQtyPolicy.Max));
        }

        if (!variant.HasStockAvailable(quantity))
        {
            throw new DomainException(E.InsufficientStock(variant.SkuCode));
        }
    }
}
