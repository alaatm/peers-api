using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Lookup.DbMap;

internal sealed class LookupValueMapping : IEntityTypeConfiguration<LookupValue>
{
    public void Configure(EntityTypeBuilder<LookupValue> builder)
    {
        builder.HasAlternateKey(v => new { v.TypeId, v.Id });
        builder.HasIndex(p => new { p.TypeId, p.Key }).IsUnique();
        builder.Property(p => p.Key).HasMaxLength(64).IsUnicode(false);

        builder.ToTable(nameof(LookupValue).Underscore(), "lookup");
    }
}
