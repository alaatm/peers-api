using System.Text.Json;
using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Catalog.DbMap;

internal sealed class AttributeDefinitionMapping : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.HasIndex(p => new { p.ProductTypeId, p.Key }).IsUnique();
        builder.HasIndex(p => new { p.ProductTypeId, p.Position });

        builder.Property(p => p.Key).HasMaxLength(64).IsUnicode(true);

        builder
            .HasDiscriminator(a => a.Kind)
            .HasValue<IntAttributeDefinition>(AttributeKind.Int)
            .HasValue<DecimalAttributeDefinition>(AttributeKind.Decimal)
            .HasValue<StringAttributeDefinition>(AttributeKind.String)
            .HasValue<BoolAttributeDefinition>(AttributeKind.Bool)
            .HasValue<DateAttributeDefinition>(AttributeKind.Date)
            .HasValue<GroupAttributeDefinition>(AttributeKind.Group)
            .HasValue<EnumAttributeDefinition>(AttributeKind.Enum)
            .HasValue<LookupAttributeDefinition>(AttributeKind.Lookup);

        builder.ToTable(nameof(AttributeDefinition).Underscore(), "catalog", p =>
        {
            var isVariantProp = nameof(EnumAttributeDefinition.IsVariant);
            var lookupTypeIdProp = nameof(LookupAttributeDefinition.LookupTypeId);
            var kindCol = nameof(AttributeDefinition.Kind).Underscore();
            var isVariantCol = isVariantProp.Underscore();
            var lookupTypeIdCol = lookupTypeIdProp.Underscore();

            var allowedVariantAttributeKinds = string.Join(',',
            [
                (int)AttributeKind.Int,
                (int)AttributeKind.Decimal,
                (int)AttributeKind.Group,
                (int)AttributeKind.Enum,
                (int)AttributeKind.Lookup,
            ]);

            p.HasCheckConstraint($"CK_AD_Group_VariantOnly",
                $"[{kindCol}] <> {(int)AttributeKind.Group} OR {isVariantCol} = 1");

            p.HasCheckConstraint($"CK_AD_{isVariantProp}_NumericGroupEnumLookupOnly",
                $"[{isVariantCol}] = 0 OR [{kindCol}] IN ({allowedVariantAttributeKinds})");

            p.HasCheckConstraint($"CK_AD_{lookupTypeIdProp}_LookupOnly",
                $"""
                (
                    ([{kindCol}] = {(int)AttributeKind.Lookup} AND [{lookupTypeIdCol}] IS NOT NULL)
                    OR
                    ([{kindCol}] <> {(int)AttributeKind.Lookup} AND [{lookupTypeIdCol}] IS NULL)
                )
                """);
        });
    }
}

internal sealed class DependentAttributeDefinitionMapping : IEntityTypeConfiguration<DependentAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<DependentAttributeDefinition> builder) =>
        // Self-reference: depends on another definition
        builder
            .HasOne(p => p.DependsOn)
            .WithMany()
            .HasForeignKey(p => p.DependsOnId)
            .OnDelete(DeleteBehavior.Restrict);
}

internal sealed class EnumAttributeDefinitionMapping : IEntityTypeConfiguration<EnumAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<EnumAttributeDefinition> builder) =>
        builder
            .HasMany(p => p.Options)
            .WithOne(p => p.EnumAttributeDefinition)
            .HasForeignKey(p => p.EnumAttributeDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
}

internal sealed class LookupAttributeDefinitionMapping : IEntityTypeConfiguration<LookupAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<LookupAttributeDefinition> builder)
    {
        builder
            .Property(e => e.Config)
            .HasColumnName(nameof(LookupAttributeDefinition.Config).Underscore())
            .HasConversion(
                v => JsonSerializer.Serialize(v, CatalogJsonSourceGenContext.Default.LookupAttrConfig),
                s => JsonSerializer.Deserialize(s, CatalogJsonSourceGenContext.Default.LookupAttrConfig));

        builder
            .HasOne(p => p.LookupType)
            .WithMany()
            .HasForeignKey(p => p.LookupTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class GroupAttributeDefinitionMapping : IEntityTypeConfiguration<GroupAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<GroupAttributeDefinition> builder) =>
        builder
            .HasMany(p => p.Members)
            .WithOne(p => p.GroupDefinition)
            .HasForeignKey(p => p.GroupDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
}

internal sealed class IntAttributeDefinitionMapping : IEntityTypeConfiguration<IntAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<IntAttributeDefinition> builder) =>
        builder
            .Property(e => e.Config)
            .HasColumnName(nameof(NumericAttributeDefinition<>.Config).Underscore())
            .HasConversion(
                v => JsonSerializer.Serialize(v, CatalogJsonSourceGenContext.Default.NumericAttrConfigInt32),
                s => JsonSerializer.Deserialize(s, CatalogJsonSourceGenContext.Default.NumericAttrConfigInt32));
}

internal sealed class DecimalAttributeDefinitionMapping : IEntityTypeConfiguration<DecimalAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<DecimalAttributeDefinition> builder) =>
        builder
            .Property(e => e.Config)
            .HasColumnName(nameof(NumericAttributeDefinition<>.Config).Underscore())
            .HasConversion(
                v => JsonSerializer.Serialize(v, CatalogJsonSourceGenContext.Default.NumericAttrConfigDecimal),
                s => JsonSerializer.Deserialize(s, CatalogJsonSourceGenContext.Default.NumericAttrConfigDecimal));
}

internal sealed class StringAttributeDefinitionMapping : IEntityTypeConfiguration<StringAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<StringAttributeDefinition> builder)
        => builder
            .Property(e => e.Config)
            .HasColumnName(nameof(StringAttributeDefinition.Config).Underscore())
            .HasConversion(
                v => JsonSerializer.Serialize(v, CatalogJsonSourceGenContext.Default.StringAttrConfig),
                s => JsonSerializer.Deserialize(s, CatalogJsonSourceGenContext.Default.StringAttrConfig)!);
}
