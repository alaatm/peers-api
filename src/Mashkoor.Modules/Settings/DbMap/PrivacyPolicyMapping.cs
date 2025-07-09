using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Modules.Settings.Domain;

namespace Mashkoor.Modules.Settings.DbMap;

internal sealed class PrivacyPolicyMapping : IEntityTypeConfiguration<PrivacyPolicy>
{
    public void Configure(EntityTypeBuilder<PrivacyPolicy> builder)
        => builder.ToTable(nameof(PrivacyPolicy).Underscore(), "settings");
}
