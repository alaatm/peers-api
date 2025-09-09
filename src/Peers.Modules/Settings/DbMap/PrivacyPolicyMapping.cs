using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Settings.Domain;

namespace Peers.Modules.Settings.DbMap;

internal sealed class PrivacyPolicyMapping : IEntityTypeConfiguration<PrivacyPolicy>
{
    public void Configure(EntityTypeBuilder<PrivacyPolicy> builder)
        => builder.ToTable(nameof(PrivacyPolicy).Underscore(), "settings");
}
