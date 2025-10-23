using System.Diagnostics;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Domain;

/// <summary>
/// Per-product-type allow-list for lookup options, used to curate picker options (e.g., allowed brands).
/// </summary>
/// <remarks>
/// Semantics:
/// - Rows exist → this product type is curated for the given lookup type; only listed options are allowed.
/// - No rows for (ProductTypeId, TypeId) → treat as "allow all" (inherit from ancestor or show all).
/// Integrity:
/// - Composite PK (ProductTypeId, TypeId, OptionId).
/// - Composite FK (TypeId, ValueId) → (LookupOption.TypeId, LookupOption.Id) ensures the value belongs to the type.
/// Lifecycle:
/// - Rows are copied when creating a next version of the same product type; not copied to children.
/// - When the last <c>LookupAttributeDefinition</c> of a TypeId is removed, prune corresponding rows.
/// </remarks>
[DebuggerDisplay("{D,nq}")]
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
    /// The identifier of the allowed lookup option within <see cref="TypeId"/>
    /// </summary>
    public int OptionId { get; set; }
    /// <summary>
    /// The owning product type.
    /// </summary>
    public ProductType ProductType { get; set; } = default!;
    /// <summary>
    /// The allowed lookup option.
    /// </summary>
    public LookupOption Option { get; set; } = default!;

    private LookupAllowed() { }

    public LookupAllowed(ProductType productType, LookupOption option)
    {
        ProductType = productType;
        Option = option;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"{ProductType.SlugPath}: {Option.D}";
}
