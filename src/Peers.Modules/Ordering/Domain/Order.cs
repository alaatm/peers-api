using Peers.Core.Domain.Errors;
using Peers.Core.Security.StrongKeys;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Sellers.Domain;
using E = Peers.Modules.Ordering.OrderingErrors;

namespace Peers.Modules.Ordering.Domain;

/// <summary>
/// Represents a customer order.
/// </summary>
public sealed class Order : Entity, IAggregateRoot
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
    /// The unique order number (human-friendly).
    /// </summary>
    public string Number { get; private set; } = default!;
    /// <summary>
    /// The date and time when the order was placed.
    /// </summary>
    public DateTime PlacedAt { get; private set; }
    /// <summary>
    /// The current state of the order.
    /// </summary>
    public OrderState State { get; private set; }
    /// <summary>
    /// The total amount for all items in the order (excluding shipping) at the time of placement.
    /// </summary>
    public decimal ItemsTotal { get; private set; }
    /// <summary>
    /// The total shipping amount.
    /// </summary>
    public decimal ShippingFee { get; private set; }
    /// <summary>
    /// The date and time when the order was marked ready to ship.
    /// </summary>
    public DateTime? ReadyToShipAt { get; private set; }
    /// <summary>
    /// The date and time when the order was delivered (if applicable).
    /// </summary>
    public DateTime? DeliveredAt { get; private set; }
    /// <summary>
    /// The OTP code for confirming delivery on seller-managed shipments.
    /// </summary>
    private string DeliveryOtp { get; set; } = default!;
    /// <summary>
    /// The reason for order cancellation (if applicable).
    /// </summary>
    public OrderCancellationReason? CancellationReason { get; set; }
    /// <summary>
    /// The customer who placed the order.
    /// </summary>
    public Customer Buyer { get; private set; } = default!;
    /// <summary>
    /// The customer who is selling the items.
    /// </summary>
    public Seller Seller { get; private set; } = default!;
    /// <summary>
    /// The payment method used for the order.
    /// </summary>
    public PaymentMethod? PaymentMethod { get; private set; }
    /// <summary>
    /// The list of line items in the order.
    /// </summary>
    public List<OrderLine> Lines { get; private set; } = default!;

    private Order() { }

    /// <summary>
    /// Creates a new order based on the contents of the specified cart.
    /// </summary>
    /// <param name="cart">The cart containing the items and buyer information to use for creating the order. Cannot be null.</param>
    public static Order Create(Cart cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        var order = new Order()
        {
            Number = "TODO",
            PlacedAt = DateTime.UtcNow,
            State = OrderState.Placed,
            ItemsTotal = 0m,
            ShippingFee = 0m,
            DeliveryOtp = KeyGenerator.Create(6, true),
            Buyer = cart.Buyer,
            Seller = cart.Seller,
            Lines = [],
        };

        foreach (var line in cart.Lines)
        {
            order.AddLineItem(line.Variant, line.Quantity);
        }

        return order;
    }

    /// <summary>
    /// Cancels the order and records the specified cancellation reason.
    /// </summary>
    /// <param name="reason">The reason for cancelling the order.</param>
    /// <exception cref="DomainException">Thrown if the order has not been placed and therefore cannot be cancelled.</exception>
    public void Cancel(OrderCancellationReason reason)
    {
        if (State is not OrderState.Placed)
        {
            throw new DomainException(E.NotPlaced);
        }

        State = OrderState.Cancelled;
        CancellationReason = reason;
    }

    private void AddLineItem([NotNull] ListingVariant variant, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        if (State is not OrderState.Placed)
        {
            throw new DomainException(E.NotPlaced);
        }

        var listing = variant.Listing;

        if (listing.State is not ListingState.Published)
        {
            throw new DomainException(E.ListingNotPublished);
        }

        if (listing.Seller != Seller)
        {
            throw new DomainException(E.ListingSellerMismatch);
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

        var line = new OrderLine(this, variant, quantity);
        Lines.Add(line);

        ItemsTotal += line.UnitPrice * quantity;
    }
}
