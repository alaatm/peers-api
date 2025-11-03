using Peers.Core.Domain.Errors;
using static Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Ordering;

public static class OrderingErrors
{
    /// <summary>
    /// Order has not been placed yet.
    /// </summary>
    public static DomainError NotPlaced => new(Titles.CannotApplyOperation, "order.not-placed");
    /// <summary>
    /// Listing is not published.
    /// </summary>
    public static DomainError ListingNotPublished => new(Titles.CannotApplyOperation, "order.listing-not-published");
    /// <summary>
    /// All items must belong to the same seller.
    /// </summary>
    public static DomainError ListingSellerMismatch => new(Titles.CannotApplyOperation, "order.listing-seller-mismatch");
    /// <summary>
    /// The requested quantity {0} is out of the allowed range {1} to {2}.
    /// </summary>
    public static DomainError QtyOutOfRange(int requested, int min, int max) => new(Titles.ValidationFailed, "order.qty-out-of-range", requested, min, max);
    /// <summary>
    /// Insufficient stock for SKU '{0}'.
    /// </summary>
    public static DomainError InsufficientStock(string sku) => new(Titles.CannotApplyOperation, "order.insufficient-stock", sku);
    /// <summary>
    /// Invalid operation in the current state.
    /// </summary>
    public static DomainError InvalidState => new(Titles.CannotApplyOperation, "order.invalid-state");
    /// <summary>
    /// Carrier label is only valid for platform-managed shipments.
    /// </summary>
    public static DomainError LabelOnlyPlatformManaged => new(Titles.CannotApplyOperation, "order.label-only-platform");
    /// <summary>
    /// The shipment with tracking ID '{0}' was not found.
    /// </summary>
    public static DomainError ShipmentNotFound(string trackingId) => new(Titles.NotFound, "order.shipment-not-found", trackingId);

    public static class Shipments
    {
        /// <summary>
        /// Invalid state transition for dispatch.
        /// </summary>
        public static DomainError InvalidDispatchState => new(Titles.CannotApplyOperation, "order.shipments.invalid-dispatch-state");
        /// <summary>
        /// Webhooks apply to platform-managed shipments only.
        /// </summary>
        public static DomainError WebhookOnlyPlatformManaged => new(Titles.CannotApplyOperation, "order.shipments.webhook-only-platform");
        /// <summary>
        /// OTP delivery confirmation applies to seller-managed shipments only.
        /// </summary>
        public static DomainError OtpOnlySellerManaged => new(Titles.CannotApplyOperation, "order.shipments.otp-only-seller");
        /// <summary>
        /// Invalid OTP code.
        /// </summary>
        public static DomainError InvalidOtp => new(Titles.CannotApplyOperation, "order.shipments.invalid-otp");
    }
}
