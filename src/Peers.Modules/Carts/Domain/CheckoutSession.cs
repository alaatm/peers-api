using Peers.Core.Payments.Models;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Ordering.Domain;

namespace Peers.Modules.Carts.Domain;

public sealed class CheckoutSession : Entity
{
    public static readonly TimeSpan CheckoutSessionDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan IntentIssuedCheckoutSessionExpiryIncrement = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan HppCheckoutSessionDuration = CheckoutSessionDuration + IntentIssuedCheckoutSessionExpiryIncrement;

    /// <summary>
    /// The unique identity of the session.
    /// </summary>
    public Guid SessionId { get; private set; }
    /// <summary>
    /// The unique identity of the cart.
    /// </summary>
    public int CartId { get; private set; }
    /// <summary>
    /// The unique identity of the customer.
    /// </summary>
    public int CustomerId { get; private set; }
    /// <summary>
    /// The unique identity of the shipping address.
    /// </summary>
    public int ShippingAddressId { get; private set; }
    /// <summary>
    /// The unique identity of the payment method, if selected.
    /// </summary>
    public int? PaymentMethodId { get; private set; }
    /// <summary>
    /// The payment gateway reference identifier for the payment, if created.
    /// </summary>
    public string? PaymentId { get; private set; }
    /// <summary>
    /// The current status of the checkout session.
    /// </summary>
    public CheckoutSessionStatus Status { get; private set; }
    /// <summary>
    /// The payment type used in this checkout session
    /// </summary>
    public CheckoutSessionPaymentType PaymentType { get; private set; }
    /// <summary>
    /// The timestamp when the session was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// The timestamp when the session was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }
    /// <summary>
    /// The timestamp when the session expires.
    /// </summary>
    public DateTime ExpiresOn { get; private set; }
    /// <summary>
    /// The total shipping amount.
    /// </summary>
    public decimal ShippingFee { get; private set; }
    /// <summary>
    /// The cart associated with the session.
    /// </summary>
    public Cart Cart { get; private set; } = default!;
    /// <summary>
    /// The customer who is checking out.
    /// </summary>
    public Customer Customer { get; private set; } = default!;
    /// <summary>
    /// The shipping address provided for the order.
    /// </summary>
    public CustomerAddress ShippingAddress { get; private set; } = default!;
    /// <summary>
    /// The payment method selected for the order.
    /// </summary>
    public PaymentMethod? PaymentMethod { get; private set; } = default!;
    /// <summary>
    /// The order associated with the session, if created.
    /// </summary>
    public Order? Order { get; private set; }
    /// <summary>
    /// The list of line items in the session.
    /// </summary>
    public List<CheckoutSessionLine> Lines { get; private set; } = default!;

    /// <summary>
    /// Returns the total amount for the order, including line items and shipping fee.
    /// </summary>
    public decimal OrderTotal => Lines.Sum(p => p.LineTotal) + ShippingFee;

    private CheckoutSession() { }

    /// <summary>
    /// Creates a new checkout session based on the contents of the specified cart.
    /// </summary>
    /// <param name="cart">The cart containing the items and buyer information to use for creating the session. Cannot be null.</param>
    /// <param name="shippingAddress">The shipping address to use for the session. Cannot be null.</param>
    /// <param name="paymentMethod">The payment method to use for the session, if any.</param>
    /// <param name="shippingFee">The total shipping fee</param>
    /// <param name="time">The timestamp to use for the session creation. Cannot be in the future.</param>
    internal CheckoutSession(
        Cart cart,
        CustomerAddress shippingAddress,
        PaymentMethod? paymentMethod,
        decimal shippingFee,
        DateTime time)
    {
        ArgumentNullException.ThrowIfNull(cart);

        SessionId = Guid.NewGuid();
        Status = CheckoutSessionStatus.Active;
        PaymentType = paymentMethod is null ? CheckoutSessionPaymentType.HostedPagePayment : CheckoutSessionPaymentType.Api;
        ShippingFee = shippingFee;
        CreatedAt = time;
        UpdatedAt = time;
        ExpiresOn = time.Add(CheckoutSessionDuration);
        Cart = cart;
        Customer = cart.Buyer;
        ShippingAddress = shippingAddress;
        PaymentMethod = paymentMethod;
        Lines = new List<CheckoutSessionLine>(cart.Lines.Count);

        foreach (var line in cart.Lines)
        {
            Lines.Add(new CheckoutSessionLine(this, line.Variant, line.Quantity));
            line.Variant.ReserveStock(line.Quantity);
        }
    }

    public bool IsExpired(DateTime time) => time >= ExpiresOn;

    public void Update(CustomerAddress deliveryAddress, PaymentMethod? paymentMethod, DateTime time)
    {
        ShippingAddress = deliveryAddress;
        PaymentMethod = paymentMethod;
        UpdatedAt = time;
    }

    public void MarkPayInProgress(string paymentId, DateTime time)
    {
        PaymentId = paymentId;
        UpdatedAt = time;
        Status = CheckoutSessionStatus.Paying;
    }

    public void MarkIntentIssued(DateTime time)
    {
        UpdatedAt = time;
        Status = CheckoutSessionStatus.IntentIssued;

        // Extend expiry upon HPP issuance
        ExpiresOn = ExpiresOn.Add(IntentIssuedCheckoutSessionExpiryIncrement);
    }

    public Order MarkCompleted(PaymentResponse payment, DateTime time)
    {
        UpdatedAt = time;
        Status = CheckoutSessionStatus.Completed;

        foreach (var line in Lines)
        {
            line.Variant.CommitReservedStock(line.Quantity);
        }

        Order = Order.Create(this, time);
        return Order;
    }

    public void MarkFailed(DateTime time)
    {
        UpdatedAt = time;
        Status = CheckoutSessionStatus.Failed;
        ReleaseReservedStock();
    }

    public void MarkExpired(DateTime time)
    {
        UpdatedAt = time;
        Status = CheckoutSessionStatus.Expired;
        ReleaseReservedStock();
    }

    public void Invalidate(DateTime time)
    {
        UpdatedAt = time;
        Status = CheckoutSessionStatus.Invalidated;
        ReleaseReservedStock();
    }

    private void ReleaseReservedStock()
    {
        foreach (var line in Lines)
        {
            line.Variant.ReleaseReservedStock(line.Quantity);
        }
    }
}
