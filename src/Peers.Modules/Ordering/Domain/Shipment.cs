using Peers.Core.Domain.Errors;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Media.Domain;
using E = Peers.Modules.Ordering.OrderingErrors;

namespace Peers.Modules.Ordering.Domain;

// Physical handoff of items to fulfill an order
public sealed class Shipment : Entity
{
    /// <summary>
    /// Order identity.
    /// </summary>
    public int OrderId { get; private set; }
    /// <summary>
    /// The shipping mode
    /// </summary>
    public ShipmentMode ShippingMode { get; private set; }
    /// <summary>
    /// The current state of the shipment.
    /// </summary>
    public ShipmentState State { get; private set; }
    /// <summary>
    /// The integrated carrier for platform-managed shipments.
    /// </summary>
    public CarrierType? Carrier { get; private set; }
    /// <summary>
    /// The internal tracking ID for all types of shipments (platform- and seller-managed).
    /// </summary>
    public string TrackingId { get; private set; }
    /// <summary>
    /// The carrier waybill / tracking number for platform-managed.
    /// </summary>
    public string? Waybill { get; private set; }
    /// <summary>
    /// The 3rd-party tracking carrier (seller-managed).
    /// </summary>
    public string? TrackingCarrier { get; private set; }
    /// <summary>
    /// The 3rd-party tracking code (seller-managed).
    /// </summary>
    public string? TrackingCode { get; private set; }
    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Dispatch timestamp.
    /// </summary>
    public DateTime? DispatchedAt { get; private set; }
    /// <summary>
    /// Delivered timestamp.
    /// </summary>
    public DateTime? DeliveredAt { get; private set; }
    public Order Order { get; private set; }
    /// <summary>
    /// The list of shipment proof media (seller-managed).
    /// </summary>
    public List<MediaFile> ProofPhotos { get; private set; }

    private Shipment() { }

    // Create a new shipment for the given order.
    internal Shipment(Order order, FulfillmentMethod fulfillmentMethod, DateTime date)
    {
        Order = order;
        ShippingMode = fulfillmentMethod.ToShippingMode();
        State = ShipmentState.Draft;
        CreatedAt = date;
        ProofPhotos = [];
    }

    /// <summary>
    /// Assigns the specified carrier to the shipment and generates a shipping label using the provided waybill number.
    /// This operation is only supported for platform-managed shipments in the draft state.
    /// </summary>
    /// <param name="carrier">The carrier to assign to the shipment.</param>
    /// <param name="waybill">The waybill number to associate with the generated shipping label.</param>
    public void AssignCarrierAndGenerateLabel(CarrierType carrier, string waybill)
    {
        if (ShippingMode is not ShipmentMode.PlatformManaged)
        {
            throw new DomainException(E.LabelOnlyPlatformManaged);
        }

        if (State is not ShipmentState.Draft)
        {
            throw new DomainException(E.Shipments.InvalidDispatchState);
        }

        Carrier = carrier;
        Waybill = waybill;
        State = ShipmentState.LabelReady;
    }

    /// <summary>
    /// Marks the shipment as dispatched and updates its state to in transit.
    /// </summary>
    /// <remarks>
    /// For platform-managed shipments, the shipment must be in the 'LabelReady' state to be
    /// dispatched. For seller-managed shipments, the shipment must be in the 'Draft' state. After calling this method,
    /// the shipment's dispatch timestamp is set to the current UTC time, and its state is updated to
    /// 'InTransit'.
    /// </remarks>
    public void MarkDispatched(DateTime date)
    {
        if (ShippingMode is ShipmentMode.PlatformManaged &&
            State is not ShipmentState.LabelReady)
        {
            throw new DomainException(E.Shipments.InvalidDispatchState);
        }

        if (ShippingMode is ShipmentMode.SellerManaged &&
            State is not ShipmentState.Draft)
        {
            throw new DomainException(E.Shipments.InvalidDispatchState);
        }

        DispatchedAt = date;
        State = ShipmentState.InTransit;
    }

    // Applies a carrier webhook update (platform-managed only).
    public void ApplyCarrierWebhook(string status)
    {
        if (ShippingMode is not ShipmentMode.PlatformManaged)
        {
            throw new DomainException(E.Shipments.WebhookOnlyPlatformManaged);
        }

        var s = status?.Trim().ToLowerInvariant();
        switch (s)
        {
            case "intransit":
            case "outfordelivery":
                if (State is ShipmentState.LabelReady or ShipmentState.InTransit)
                {
                    State = ShipmentState.InTransit;
                }

                break;
            case "delivered":
                DeliveredAt = DateTime.UtcNow;
                State = ShipmentState.Delivered;
                break;
            case "failedattempt":
            case "exception":
                State = ShipmentState.DeliveryFailed;
                break;
            default:
                // ignore or audit
                break;
        }
    }

    // Sets seller-managed tracking info.
    public void ProvideTracking(string carrier, string code)
    {
        if (ShippingMode is not ShipmentMode.SellerManaged)
        {
            throw new DomainException(E.Shipments.OtpOnlySellerManaged);
        }

        TrackingCarrier = carrier;
        TrackingCode = code;
    }

    // Uploads a seller-managed proof photo.
    public void AddShipmentProofMedia(MediaFile file)
    {
        if (ShippingMode is not ShipmentMode.SellerManaged)
        {
            throw new DomainException(E.Shipments.OtpOnlySellerManaged);
        }

        ProofPhotos.Add(file);
    }

    /// <summary>
    /// Confirms delivery by OTP (seller-managed only) which marks the shipment as delivered at the specified date.
    /// </summary>
    /// <param name="deliveryOtp">The expected OTP for delivery confirmation.</param>
    /// <param name="providedOtp">The provided OTP for validation.</param>
    /// <param name="date">The date and time when the shipment was delivered.</param>
    public void ConfirmDelivery(string deliveryOtp, string providedOtp, DateTime date)
    {
        if (ShippingMode is not ShipmentMode.SellerManaged)
        {
            throw new DomainException(E.Shipments.OtpOnlySellerManaged);
        }

        if (State != ShipmentState.InTransit)
        {
            throw new DomainException(E.Shipments.InvalidDispatchState);
        }

        if (!string.Equals(deliveryOtp, providedOtp, StringComparison.Ordinal))
        {
            throw new DomainException(E.Shipments.InvalidOtp);
        }

        DeliveredAt = date;
        State = ShipmentState.Delivered;
    }
}
