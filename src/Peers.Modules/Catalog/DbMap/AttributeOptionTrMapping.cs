using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.I18n.DbMap;

namespace Peers.Modules.Catalog.DbMap;

internal sealed class AttributeOptionTrMapping : TranslationBaseMapping<AttributeOption, AttributeOptionTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<AttributeOptionTr> builder)
        => builder.ToTable(nameof(AttributeOptionTr).Underscore(), "i18n");
}
