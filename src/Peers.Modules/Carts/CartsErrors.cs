using Peers.Core.Domain.Errors;
using static Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Carts;

public static class CartsErrors
{
    /// <summary>
    /// Buyer and seller cannot be the same.
    /// </summary>
    public static DomainError BuyerSellerSame => new(Titles.CannotApplyOperation, "carts.buyer-seller-same");
    /// <summary>
    /// Listing is not published.
    /// </summary>
    public static DomainError ListingNotPublished => new(Titles.CannotApplyOperation, "carts.listing-not-published");
    /// <summary>
    /// All items must belong to the same seller.
    /// </summary>
    public static DomainError ListingSellerMismatch => new(Titles.CannotApplyOperation, "carts.listing-seller-mismatch");
    /// <summary>
    /// The variant with key '{0}' was not found.
    /// </summary>
    public static DomainError VariantNotFound(string variantKey) => new(Titles.NotFound, "carts.variant-not-found", variantKey);
    /// <summary>
    /// The line item was not found in the cart.
    /// </summary>
    public static DomainError LineNotFound => new(Titles.NotFound, "carts.line-not-found");
    /// <summary>
    /// The requested quantity {0} is out of the allowed range {1} to {2}.
    /// </summary>
    public static DomainError QtyOutOfRange(int requested, int min, int max) => new(Titles.ValidationFailed, "carts.qty-out-of-range", requested, min, max);
    /// <summary>
    /// Insufficient stock for SKU '{0}'.
    /// </summary>
    public static DomainError InsufficientStock(string sku) => new(Titles.CannotApplyOperation, "carts.insufficient-stock", sku);


    public static DomainError CartNotEmpty => new(Titles.CannotApplyOperation, "carts.cart-not-empty");
    public static DomainError CustomerMismatch => new(Titles.CannotApplyOperation, "carts.customer-mismatch");
    public static DomainError OrderNotInPlacedState => new(Titles.CannotApplyOperation, "carts.order-not-in-placed-state");
}
