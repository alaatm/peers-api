namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents the data type of a product attribute.
/// </summary>
public enum AttributeKind
{
    /// <summary>
    /// An integer value.
    /// </summary>
    Int,
    /// <summary>
    /// A decimal value.
    /// </summary>
    Decimal,
    /// <summary>
    /// A string value.
    /// </summary>
    String,
    /// <summary>
    /// A boolean value.
    /// </summary>
    Bool,
    /// <summary>
    /// A date value.
    /// </summary>
    Date,
    /// <summary>
    /// A small, fixed set of choices modeled as inline options.
    /// </summary>
    /// <remarks>
    /// Used when the value set is closed and stable (e.g., condition = new/used/refurbished).
    /// Values live as inline options on the attribute itself.
    /// May depend on another Enum (e.g., size→size_system) but not on a Lookup.
    /// </remarks>
    Enum,
    /// <summary>
    /// A value chosen from a global lookup/dimension (e.g., brand, device_model).
    /// </summary>
    /// <remarks>
    /// Used when the value set is open-ended, shared across categories, or frequently updated.
    /// Values live in a separate lookup catalog; no inline options on the attribute itself.
    /// May depend on another Lookup (e.g., brand→model) but not on an Enum.
    /// </remarks>
    Lookup,
}
