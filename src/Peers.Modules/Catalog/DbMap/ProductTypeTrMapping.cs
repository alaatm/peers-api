using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.I18n.DbMap;

namespace Peers.Modules.Catalog.DbMap;

internal sealed class ProductTypeTrMapping : TranslationBaseMapping<ProductType, ProductTypeTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<ProductTypeTr> builder)
        => builder.ToTable(nameof(ProductTypeTr).Underscore(), "i18n");
}
