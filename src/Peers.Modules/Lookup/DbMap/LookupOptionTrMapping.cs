using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.I18n.DbMap;
using Peers.Modules.Lookup.Domain;
using Peers.Modules.Lookup.Domain.Translations;

namespace Peers.Modules.Lookup.DbMap;

internal sealed class LookupOptionTrMapping : TranslationBaseMapping<LookupOption, LookupOptionTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<LookupOptionTr> builder)
        => builder.ToTable(nameof(LookupOptionTr).Underscore(), "i18n");
}
