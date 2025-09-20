namespace Peers.Modules.Catalog.Domain;

/// <summary>
/// Represents the type of a product.
/// </summary>
public enum ProductTypeKind
{
    /// <summary>
    /// A physical product that can be shipped.
    /// </summary>
    Physical = 0,
    /// <summary>
    /// A digital product that can be downloaded or accessed online.
    /// </summary>
    Digital = 1,
    /// <summary>
    /// A service-based product, such as a cleaning or consulting service.
    /// </summary>
    Service = 2,
}
