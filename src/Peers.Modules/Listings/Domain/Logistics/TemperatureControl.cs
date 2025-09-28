namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Specifies the available temperature control options for storage or transport environments.
/// </summary>
public enum TemperatureControl
{
    /// <summary>
    /// Indicates that no special temperature control is required.
    /// </summary>
    None = 0,
    /// <summary>
    /// Indicates that the item is stored or shipped at a chilled temperature.
    /// </summary>
    Chilled = 1,
    /// <summary>
    /// Indicates that the item is stored or shipped at a frozen temperature.
    /// </summary>
    Frozen = 2,
}
