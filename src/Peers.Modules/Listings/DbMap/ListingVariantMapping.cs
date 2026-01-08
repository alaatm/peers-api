using System.Text.Json;
using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Attributes;
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
        builder
            .Property(e => e.SelectionSnapshot)
            .HasColumnName(nameof(ListingVariant.SelectionSnapshot).Underscore())
            .HasConversion(
                v => JsonSerializer.Serialize(v, ListingsJsonSourceGenContext.Default.VariantSelectionSnapshot),
                s => JsonSerializer.Deserialize(s, ListingsJsonSourceGenContext.Default.VariantSelectionSnapshot)!);

        // Concurrency token
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        // Parent (Listing)
        builder
            .HasOne(p => p.Listing)
            .WithMany(l => l.Variants)
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(e => e.Logistics)
            .HasConversion(
                v => JsonSerializer.Serialize(v, ListingsJsonSourceGenContext.Default.LogisticsProfile),
                s => JsonSerializer.Deserialize(s, ListingsJsonSourceGenContext.Default.LogisticsProfile)!);

        builder.OwnsMany(p => p.Attributes, nav =>
        {
            var enumOptIdColName = nameof(ListingVariantAttribute.EnumAttributeOptionId).Underscore();
            var lookupOptIdColName = nameof(ListingVariantAttribute.LookupOptionId).Underscore();
            var numericColName = nameof(ListingVariantAttribute.NumericValue).Underscore();

            // One row per (variant, attribute definition) - prevents multiple options for the same axis on a single variant
            nav.HasKey(p => new { p.ListingVariantId, p.AttributeDefinitionId });

            // Search indexes
            nav
                .HasIndex(p => new { p.AttributeDefinitionId, p.NumericValue })
                .HasDatabaseName("IX_LVA_Num")
                .HasFilter($"[{numericColName}] IS NOT NULL");
            nav
                .HasIndex(p => new { p.AttributeDefinitionId, p.EnumAttributeOptionId })
                .HasDatabaseName("IX_LVA_Enum")
                .HasFilter($"[{enumOptIdColName}] IS NOT NULL");
            nav
                .HasIndex(p => new { p.AttributeDefinitionId, p.LookupOptionId })
                .HasDatabaseName("IX_LVA_Lookup")
                .HasFilter($"[{lookupOptIdColName}] IS NOT NULL");

            nav
                .WithOwner(p => p.ListingVariant)
                .HasForeignKey(p => p.ListingVariantId);

            nav
                .HasOne(p => p.AttributeDefinition)
                .WithMany()
                .HasForeignKey(p => p.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            nav
                .HasOne(p => p.EnumAttributeOption)
                .WithMany()
                .HasForeignKey(p => p.EnumAttributeOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            nav
                .HasOne(p => p.LookupOption)
                .WithMany()
                .HasForeignKey(p => p.LookupOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            nav.ToTable(nameof(ListingVariantAttribute).Underscore(), p =>
            {
                var positionPropName = nameof(ListingVariantAttribute.Position);
                var positionColName = positionPropName.Underscore();
                var attrKindColName = nameof(ListingVariantAttribute.AttributeKind).Underscore();

                var allowedAttrKinds = string.Join(',',
                [
                    (int)AttributeKind.Int,
                    (int)AttributeKind.Decimal,
                    (int)AttributeKind.Enum,
                    (int)AttributeKind.Lookup,
                ]);
                var allowedNumericKinds = string.Join(',',
                [
                    (int)AttributeKind.Int,
                    (int)AttributeKind.Decimal,
                ]);

                p.HasCheckConstraint($"CK_LVA_{positionPropName}_NonNegative", $"[{positionColName}] >= 0");
                p.HasCheckConstraint("CK_LVA_ValidValueCombination",
                $"""
                ([{attrKindColName}] IN ({allowedAttrKinds}))
                AND (
                    ([{attrKindColName}] IN ({allowedNumericKinds}) AND [{numericColName}] IS NOT NULL AND [{enumOptIdColName}] IS NULL AND [{lookupOptIdColName}] IS NULL)
                 OR ([{attrKindColName}] = {(int)AttributeKind.Enum} AND [{enumOptIdColName}] IS NOT NULL AND [{numericColName}] IS NULL AND [{lookupOptIdColName}] IS NULL)
                 OR ([{attrKindColName}] = {(int)AttributeKind.Lookup} AND [{lookupOptIdColName}] IS NOT NULL AND [{numericColName}] IS NULL AND [{enumOptIdColName}] IS NULL)
                )                
                """);
            });
        });

        builder.ToTable(nameof(ListingVariant).Underscore());
        builder.Navigation(p => p.Attributes).AutoInclude(false);
    }
}
