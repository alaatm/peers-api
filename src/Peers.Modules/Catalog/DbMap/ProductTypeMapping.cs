using System.Text.Json;
using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain;

namespace Peers.Modules.Catalog.DbMap;

internal sealed class ProductTypeMapping : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.HasIndex(p => p.SlugPath).IsUnique();
        builder.HasIndex(p => new { p.ParentId, p.Slug, p.Version }).IsUnique();
        builder.HasIndex(p => new { p.IsSelectable, p.ParentId });

        builder.Property(p => p.Slug).HasMaxLength(64).IsUnicode(false);
        builder.Property(p => p.SlugPath).HasMaxLength(512).IsUnicode(false);

        builder
            .HasOne(p => p.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(p => p.Index, nav =>
        {
            nav.HasKey(p => p.ProductTypeId);

            nav
                .WithOwner(p => p.ProductType)
                .HasForeignKey(p => p.ProductTypeId);

            nav
                .Property(e => e.Snapshot)
                .HasColumnName(nameof(ProductTypeIndex.Snapshot).Underscore())
                .HasConversion(
                    v => JsonSerializer.Serialize(v, CatalogJsonSourceGenContext.Default.CatalogIndexSnapshot),
                    s => JsonSerializer.Deserialize(s, CatalogJsonSourceGenContext.Default.CatalogIndexSnapshot)!);

            nav.ToTable(nameof(ProductTypeIndex).Underscore(), "catalog");
        });

        builder
            .HasMany(p => p.Attributes)
            .WithOne(p => p.ProductType)
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Index).AutoInclude(false);
        builder.ToTable(nameof(ProductType).Underscore(), "catalog");
    }
}
