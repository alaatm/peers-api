using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.I18n.DbMap;
using Peers.Modules.Lookup.Domain;
using Peers.Modules.Lookup.Domain.Translations;

namespace Peers.Modules.Lookup.DbMap;

internal sealed class LookupTypeTrMapping : TranslationBaseMapping<LookupType, LookupTypeTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<LookupTypeTr> builder)
        => builder.ToTable(nameof(LookupTypeTr).Underscore(), "i18n");
}
