using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Lookup.DbMap;

internal sealed class LookupTypeMapping : IEntityTypeConfiguration<LookupType>
{
    public void Configure(EntityTypeBuilder<LookupType> builder)
    {
        builder.HasIndex(p => p.Key).IsUnique();
        builder.Property(p => p.Key).HasMaxLength(64).IsUnicode(false);

        builder
            .HasMany(p => p.Options)
            .WithOne(p => p.Type)
            .HasForeignKey(p => p.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(p => p.ParentLinks)
            .WithOne(p => p.ParentType)
            .HasForeignKey(p => p.ParentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(p => p.ChildLinks)
            .WithOne(p => p.ChildType)
            .HasForeignKey(p => p.ChildTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(nameof(LookupType).Underscore(), "lookup");
    }
}
