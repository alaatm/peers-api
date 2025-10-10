using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Lookup.DbMap;

internal sealed class LookupOptionMapping : IEntityTypeConfiguration<LookupOption>
{
    public void Configure(EntityTypeBuilder<LookupOption> builder)
    {
        builder.HasAlternateKey(v => new { v.TypeId, v.Id });
        builder.HasIndex(p => new { p.TypeId, p.Code }).IsUnique();
        builder.Property(p => p.Code).HasMaxLength(64).IsUnicode(false);

        builder.ToTable(nameof(LookupOption).Underscore(), "lookup");
    }
}
