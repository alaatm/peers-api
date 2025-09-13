using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.I18n.DbMap;
using Peers.Modules.Settings.Domain;

namespace Peers.Modules.Settings.DbMap;

internal sealed class PrivacyPolicyTrMapping : TranslationBaseMapping<PrivacyPolicy, PrivacyPolicyTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<PrivacyPolicyTr> builder)
    {
        builder.Property(p => p.Body).Metadata.RemoveAnnotation("MaxLength");
        builder.ToTable(nameof(PrivacyPolicyTr).Underscore(), "i18n");
    }
}
