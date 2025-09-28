using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Listings.Domain.Logistics;

namespace Peers.Modules.Listings.DbMap;

internal sealed class ListingVariantMapping : IEntityTypeConfiguration<ListingVariant>
{
    public void Configure(EntityTypeBuilder<ListingVariant> builder)
    {
        builder.HasIndex(p => p.IsActive);
        // Per-listing uniqueness
        builder.HasIndex(p => new { p.ListingId, p.VariantKey }).IsUnique();
        builder.HasIndex(p => new { p.ListingId, p.SkuCode }).IsUnique();

        builder.Property(p => p.VariantKey).HasMaxLength(256);
        builder.Property(p => p.SkuCode).HasMaxLength(128);

        // Concurrency token
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        // Parent (Listing)
        builder
            .HasOne(p => p.Listing)
            .WithMany(l => l.Variants)
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ComplexProperty(p => p.Logistics, nav =>
        {
            nav.ComplexProperty(p => p.Dimensions, nav =>
            {
                nav.Property(p => p.Length).HasColumnName($"logi_dim_{nameof(Dimensions.Length).Underscore()}");
                nav.Property(p => p.Width).HasColumnName($"logi_dim_{nameof(Dimensions.Width).Underscore()}");
                nav.Property(p => p.Height).HasColumnName($"logi_dim_{nameof(Dimensions.Height).Underscore()}");
            });
            nav.Property(p => p.Weight).HasColumnName($"logi_{nameof(LogisticsProfile.Weight).Underscore()}").HasColumnType("decimal(10,3)");
            nav.Property(p => p.Fragile).HasColumnName($"logi_{nameof(LogisticsProfile.Fragile).Underscore()}");
            nav.Property(p => p.Hazmat).HasColumnName($"logi_{nameof(LogisticsProfile.Hazmat).Underscore()}");
            nav.Property(p => p.TemperatureControl).HasColumnName($"logi_{nameof(LogisticsProfile.TemperatureControl).Underscore()}");
        });

        builder.OwnsMany(p => p.Attributes, nav =>
        {
            // One row per (variant, attribute definition) - prevents multiple options for the same axis on a single variant
            nav.HasKey(p => new { p.ListingVariantId, p.AttributeDefinitionId });
            // per-variant uniqueness for chosen option
            nav.HasIndex(p => new { p.ListingVariantId, p.AttributeOptionId }).IsUnique();

            nav.WithOwner(p => p.ListingVariant)
               .HasForeignKey(p => p.ListingVariantId);

            nav.HasOne(p => p.AttributeDefinition)
               .WithMany()
               .HasForeignKey(p => p.AttributeDefinitionId)
               .OnDelete(DeleteBehavior.Restrict);

            nav.HasOne(p => p.EnumAttributeOption)
               .WithMany()
               .HasForeignKey(p => p.AttributeOptionId)
               .OnDelete(DeleteBehavior.Restrict);

            nav.ToTable(nameof(ListingVariantAttribute).Underscore(), p =>
            {
                var positionPropName = nameof(ListingAttribute.Position);
                var positionColName = positionPropName.Underscore();

                p.HasCheckConstraint($"CK_LVA_{positionPropName}_NonNegative", $"[{positionColName}] >= 0");
            });
        });

        builder.ToTable(nameof(ListingVariant).Underscore());
        builder.Navigation(p => p.Attributes).AutoInclude(false);
    }
}
