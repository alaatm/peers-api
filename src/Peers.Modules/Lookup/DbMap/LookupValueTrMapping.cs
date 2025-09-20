using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.I18n.DbMap;
using Peers.Modules.Lookup.Domain;
using Peers.Modules.Lookup.Domain.Translations;

namespace Peers.Modules.Lookup.DbMap;

internal sealed class LookupValueTrMapping : TranslationBaseMapping<LookupValue, LookupValueTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<LookupValueTr> builder)
        => builder.ToTable(nameof(LookupValueTr).Underscore(), "i18n");
}
