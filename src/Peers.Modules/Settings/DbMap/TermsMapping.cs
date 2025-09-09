using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Settings.Domain;

namespace Peers.Modules.Settings.DbMap;

internal sealed class TermsMapping : IEntityTypeConfiguration<Terms>
{
    public void Configure(EntityTypeBuilder<Terms> builder)
        => builder.ToTable(nameof(Terms).Underscore(), "settings");
}
