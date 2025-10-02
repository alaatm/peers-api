namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Specifies the types of seller-managed shipping rate calculations available for an order or shipment.
/// </summary>
/// <remarks>
/// Uses to indicate how shipping rates are determined for <see cref="FulfillmentMethod.SellerManaged"/> listings.
/// The available kinds include flat rates, rates based on weight or distance, and rates that require a
/// custom quote. The selected value may affect how shipping options are presented to buyers and how shipping costs are
/// calculated.
/// </remarks>
public enum SellerManagedRateKind
{
    /// <summary>
    /// A fixed shipping rate that does not vary based on order details.
    /// </summary>
    Flat,
    /// <summary>
    /// A shipping rate that varies based on the weight of the order.
    /// </summary>
    Weight,
    /// <summary>
    /// A shipping rate that varies based on the distance to the delivery location.
    /// </summary>
    Distance,
    /// <summary>
    /// A shipping rate that requires a custom quote from the seller.
    /// </summary>
    Quote,
}
