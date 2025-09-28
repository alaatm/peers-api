namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Specifies the party responsible for paying shipping costs in a transaction.
/// </summary>
public enum ShippingPayer
{
    /// <summary>
    /// Not applicable. i.e. for digital goods or services or non-returnable items.
    /// </summary>
    None,
    /// <summary>
    /// The buyer pays.
    /// </summary>
    Buyer,
    /// <summary>
    /// The seller pays.
    /// </summary>
    Seller,
}
