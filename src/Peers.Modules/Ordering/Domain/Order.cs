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
    /// The checkout session associated with the order.
    /// </summary>
    public CheckoutSession CheckoutSession { get; private set; } = default!;
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
    ///// <summary>
    ///// The list of shipments associated with the order.
    ///// </summary>
    //public List<Shipment> Shipments { get; private set; } = default!;
    ///// <summary>
    ///// The list of delivery complaints opened under this order.
    ///// </summary>
    //public List<DeliveryComplaint> Complaints { get; private set; } = default!;

    private Order() { }

    /// <summary>
    /// Creates a new order based on the contents of the specified checkout session.
    /// </summary>
    /// <param name="checkoutSession">The checkout session containing the items and buyer information to use for creating the order. Cannot be null.</param>
    /// <param name="time">The timestamp to use for the order placement. Cannot be in the future.</param>
    public static Order Create(CheckoutSession checkoutSession, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(checkoutSession);

        var order = new Order()
        {
            Number = "TODO",
            PlacedAt = time,
            State = OrderState.Placed,
            ItemsTotal = 0m,
            ShippingFee = 0m,
            DeliveryOtp = KeyGenerator.Create(6, true),
            Buyer = checkoutSession.Cart.Buyer,
            Seller = checkoutSession.Cart.Seller,
            CheckoutSession = checkoutSession,
            Lines = [],
            //Shipments = [],
            //Complaints = [],
        };

        foreach (var line in checkoutSession.Lines)
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

    // Sets the final shipping fee (SAR) after rating/quote acceptance.
    public void SetShippingFee(decimal fee)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fee);

        if (State is not OrderState.Placed)
        {
            throw new DomainException(E.NotPlaced);
        }

        ShippingFee = fee;
    }

    //// Marks the order ready to ship.
    //public void MarkReadyToShip()
    //{
    //    if (State is not OrderState.Placed)
    //    {
    //        throw new DomainException(E.NotPlaced);
    //    }

    //    ReadyToShipAt = DateTime.UtcNow;
    //    State = OrderState.ReadyToShip;
    //}

    //// Creates shipments by grouping lines according to shipping mode (one shipment per mode).
    //public IReadOnlyList<Shipment> CreateShipments(DateTime date)
    //{
    //    if (State is not OrderState.ReadyToShip)
    //    {
    //        throw new DomainException(E.InvalidState);
    //    }

    //    foreach (var g in Lines.GroupBy(p => p.Listing.FulfillmentPreferences.Method))
    //    {
    //        Shipments.Add(new Shipment(this, g.Key, date));
    //    }

    //    // Enter fulfillment phase
    //    State = OrderState.InTransit;
    //    return Shipments.AsReadOnly();
    //}

    //// Checks if dispatch SLA is breached.
    //public bool IsDispatchBreached([NotNull] OrderPolicyConfig policy, DateTime date)
    //    => State is OrderState.ReadyToShip &&
    //       ReadyToShipAt.HasValue &&
    //       date - ReadyToShipAt.Value > policy.DispatchSla;

    //// Marks the order as dispatch-breached.
    //public void MarkDispatchBreached()
    //{
    //    if (State is not OrderState.ReadyToShip)
    //    {
    //        throw new DomainException(E.InvalidState);
    //    }

    //    State = OrderState.DispatchBreached;
    //}

    //// Attempts to mark order delivered when all shipments are delivered.
    //public void TryMarkDeliveredWhenAllShipmentsDelivered()
    //{
    //    if (Shipments.Count == 0)
    //    {
    //        return;
    //    }

    //    if (Shipments.All(p => p.State is ShipmentState.Delivered or ShipmentState.Closed))
    //    {
    //        DeliveredAt = DateTime.UtcNow;
    //        State = OrderState.Delivered;
    //    }
    //}


    //// Returns a shipment by id or throws.
    //private Shipment GetShipment(string trackingId)
    //    => Shipments.Find(s => s.TrackingId == trackingId) ??
    //       throw new DomainException(E.ShipmentNotFound(trackingId));

    //// ---- Shipment operations (delegates that enforce the aggregate boundary) ----

    //// Assigns carrier + label on a platform-managed shipment.
    //public void AssignCarrierAndGenerateLabel(string trackingId, CarrierType carrier, string waybill)
    //    => GetShipment(trackingId).AssignCarrierAndGenerateLabel(carrier, waybill);

    //// Marks a shipment as dispatched.
    //public void MarkShipmentDispatched(string trackingId, DateTime date)
    //{
    //    var s = GetShipment(trackingId);
    //    s.MarkDispatched(date);
    //}

    //// Applies a carrier webhook to a platform-managed shipment.
    //public void ApplyCarrierWebhook(string trackingId, string status)
    //{
    //    var s = GetShipment(trackingId);
    //    s.ApplyCarrierWebhook(status);
    //    if (s.State == ShipmentState.Delivered)
    //    {
    //        TryMarkDeliveredWhenAllShipmentsDelivered();
    //    }
    //}

    //// Sets tracking for a seller-managed shipment.
    //public void ProvideTracking(string trackingId, string carrier, string code)
    //    => GetShipment(trackingId).ProvideTracking(carrier, code);

    //// Uploads proof photo on a seller-managed shipment.
    //public void UploadShipmentProof(string trackingId, MediaFile file)
    //    => GetShipment(trackingId).AddShipmentProofMedia(file);

    //// Confirms delivery by OTP on a seller-managed shipment.
    //public void ConfirmShipmentDeliveryByOtp(string trackingId, string otp, DateTime date)
    //{
    //    var s = GetShipment(trackingId);
    //    s.ConfirmDelivery(DeliveryOtp, otp, date);
    //    if (s.State == ShipmentState.Delivered)
    //    {
    //        TryMarkDeliveredWhenAllShipmentsDelivered();
    //    }
    //}
}
