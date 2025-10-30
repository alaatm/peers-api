namespace Peers.Modules.Lookup.Domain;

/// <summary>
/// Default constraint policy for lookup types when used as lookup attributes across product types.
/// </summary>
/// <remarks>
/// This value sets the requirement for product types that reference this <see cref="LookupType"/>. Individual
/// <see cref="Catalog.Domain.Attributes.LookupAttributeDefinition"/> instances may override this default
/// via their own constraint mode.
/// </remarks>
public enum LookupConstraintMode
{
    /// <summary>
    /// No allow-list is required by default. Any value belonging to the
    /// <see cref="LookupType"/> is permitted unless a lookup attribute explicitly
    /// declares an allow-list.
    /// </summary>
    /// <remarks>
    /// Good for broad, standardized dictionaries where per-category scoping
    /// adds little value (e.g., ISO language codes, country codes, plug types).
    /// </remarks>
    Open,
    /// <summary>
    /// An allow-list is required by default. Lookup attributes must declare an
    /// allow-list before values of this <see cref="LookupType"/> can be used.
    /// </summary>
    /// <remarks>
    /// Use for domains where choices must be curated per category
    /// (e.g., Brand under Mobile Phones to exclude non-phone brands).
    /// </remarks>
    RequireAllowList,
}
