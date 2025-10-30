using System.ComponentModel.DataAnnotations.Schema;
using Peers.Modules.Catalog.Domain.Index;

namespace Peers.Modules.Catalog.Domain;

public sealed class ProductTypeIndex
{
    public int ProductTypeId { get; private set; }
    public CatalogIndexSnapshot Snapshot { get; private set; } = default!;
    public ProductType ProductType { get; private set; } = default!;
    [NotMapped]
    public CatalogIndex Hydrated => field ??= Snapshot.Hydrate(ProductType);

    private ProductTypeIndex()
    {
    }

    internal static ProductTypeIndex Build(ProductType pt) => new()
    {
        ProductType = pt,
        Snapshot = CatalogIndexSnapshot.Build(pt)
    };
}
