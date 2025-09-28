namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Specifies the method by which order fulfillment is managed for a listing.
/// </summary>
/// <remarks>
/// This enum is used to indicate whether fulfillment is handled by the platform or by the seller. The
/// value affects how shipping, tracking, and customer service responsibilities are assigned.
/// </remarks>
public enum FulfillmentMethod
{
    /// <summary>
    /// Not applicable. i.e. for digital goods or services.
    /// </summary>
    None,
    /// <summary>
    /// Shipping and fulfillment are managed by the platform using integrated carriers and logistics services.
    /// </summary>
    PlatformManaged,
    /// <summary>
    /// Shipping and fulfillment are managed by the seller using their own carriers and logistics processes.
    /// </summary>
    SellerManaged,
}
