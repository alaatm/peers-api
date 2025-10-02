namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Specifies the party responsible for paying shipping costs in a transaction.
/// </summary>
public enum ShippingCostPayer
{
    /// <summary>
    /// The buyer pays.
    /// </summary>
    Buyer,
    /// <summary>
    /// The seller pays.
    /// </summary>
    Seller,
}
