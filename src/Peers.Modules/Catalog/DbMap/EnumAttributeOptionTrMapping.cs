using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.I18n.DbMap;

namespace Peers.Modules.Catalog.DbMap;

internal sealed class EnumAttributeOptionTrMapping : TranslationBaseMapping<EnumAttributeOption, EnumAttributeOptionTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<EnumAttributeOptionTr> builder)
        => builder.ToTable(nameof(EnumAttributeOptionTr).Underscore(), "i18n");
}
