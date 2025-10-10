using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Catalog.DbMap;

internal sealed class EnumAttributeOptionMapping : IEntityTypeConfiguration<EnumAttributeOption>
{
    public void Configure(EntityTypeBuilder<EnumAttributeOption> builder)
    {
        builder.HasIndex(p => new { p.EnumAttributeDefinitionId, p.Code }).IsUnique();
        builder.HasIndex(p => new { p.EnumAttributeDefinitionId, p.Position }).IsUnique();

        builder.Property(p => p.Code).HasMaxLength(64).IsUnicode(true);

        // Self-reference: scope to another option
        builder.HasOne(p => p.ParentOption)
            .WithMany()
            .HasForeignKey(p => p.ParentOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(nameof(EnumAttributeOption).Underscore());
    }
}
