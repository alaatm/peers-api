namespace Peers.Modules.Catalog.Domain;

/// <summary>
/// Represents the state of a product type.
/// </summary>
public enum ProductTypeState
{
    /// <summary>
    /// A draft state indicates that the product type is still being worked on and is not yet finalized.
    /// </summary>
    Draft = 0,
    /// <summary>
    /// A published state indicates that the product type is finalized and available for use and does not allow further modifications.
    /// </summary>
    Published = 1,
    /// <summary>
    /// A deprecated state indicates that the product type is outdated and should not be used for new products, but may still be in use by existing products.
    /// </summary>
    Deprecated = 2,
    /// <summary>
    /// A retired state indicates that the product type is no longer in use and has been removed from the system.
    /// </summary>
    Retired = 3,
}
