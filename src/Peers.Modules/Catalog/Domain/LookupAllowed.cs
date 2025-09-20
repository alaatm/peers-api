using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Domain;

/// <summary>
/// Per-product-type allow-list for lookup values, used to curate picker options (e.g., allowed brands).
/// </summary>
/// <remarks>
/// Semantics:
/// - Rows exist → this product type is curated for the given lookup type; only listed values are allowed.
/// - No rows for (ProductTypeId, TypeId) → treat as "allow all" (inherit from ancestor or show all).
/// Integrity:
/// - Composite PK (ProductTypeId, TypeId, ValueId).
/// - Composite FK (TypeId, ValueId) → (LookupValue.TypeId, LookupValue.Id) ensures the value belongs to the type.
/// Lifecycle:
/// - Rows are copied when creating a next version of the same product type; not copied to children.
/// - When the last <c>LookupAttributeDefinition</c> of a TypeId is removed, prune corresponding rows.
/// </remarks>
public sealed class LookupAllowed
{
    /// <summary>
    /// The identifier of the associated product type.
    /// </summary>
    public int ProductTypeId { get; set; }
    /// <summary>
    /// The identifier of the lookup type being curated.
    /// </summary>
    public int TypeId { get; set; }
    /// <summary>
    /// The identifier of the allowed lookup value within <see cref="TypeId"/>
    /// </summary>
    public int ValueId { get; set; }

    /// <summary>
    /// The owning product type.
    /// </summary>
    public ProductType ProductType { get; set; } = default!;
    /// <summary>
    /// The allowed lookup value.
    /// </summary>
    public LookupValue Value { get; set; } = default!;

    private LookupAllowed() { }

    public LookupAllowed(ProductType productType, LookupValue value)
    {
        ProductType = productType;
        Value = value;
    }
}
