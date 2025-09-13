using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.I18n.DbMap;
using Peers.Modules.Settings.Domain;

namespace Peers.Modules.Settings.DbMap;

internal sealed class TermsTrMapping : TranslationBaseMapping<Terms, TermsTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<TermsTr> builder)
    {
        builder.Property(p => p.Body).Metadata.RemoveAnnotation("MaxLength");
        builder.ToTable(nameof(TermsTr).Underscore(), "i18n");
    }
}
