using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain;

namespace Peers.Modules.Catalog.DbMap;

internal sealed class LookupAllowedMapping : IEntityTypeConfiguration<LookupAllowed>
{
    public void Configure(EntityTypeBuilder<LookupAllowed> builder)
    {
        builder.HasKey(p => new { p.ProductTypeId, p.TypeId, p.ValueId });
        builder.HasIndex(p => new { p.ProductTypeId, p.TypeId });

        builder
            .HasOne(p => p.ProductType)
            .WithMany(p => p.LookupsAllowed)
            .HasForeignKey(c => c.ProductTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(p => p.Value)
            .WithMany()
            .HasForeignKey(p => new { p.TypeId, p.ValueId })
            .HasPrincipalKey(p => new { p.TypeId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(nameof(LookupAllowed).Underscore(), "catalog");
    }
}
