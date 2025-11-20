using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Sellers.Domain;

namespace Peers.Modules.Sellers.DbMap;

internal sealed class ShippingProfileMapping : IEntityTypeConfiguration<ShippingProfile>
{
    public void Configure(EntityTypeBuilder<ShippingProfile> builder)
    {
        builder
            .HasIndex(p => new { p.SellerId, p.Name })
            .IsUnique(true);

        builder.ComplexProperty(p => p.FreeShippingPolicy, p =>
        {
            p.Property(p => p.MinOrder).HasColumnName($"fsp_{nameof(FreeShippingPolicy.MinOrder)}".Underscore());
            p.Property(p => p.MaxDistance).HasColumnName($"fsp_{nameof(FreeShippingPolicy.MaxDistance)}".Underscore());
        });
        builder.ComplexProperty(p => p.Rate, p =>
        {
            p.Property(p => p.Kind).HasColumnName($"r_{nameof(SellerManagedRate.Kind)}".Underscore());
            p.Property(p => p.FlatAmount).HasColumnName($"r_{nameof(SellerManagedRate.FlatAmount)}".Underscore());
            p.Property(p => p.BaseFee).HasColumnName($"r_{nameof(SellerManagedRate.BaseFee)}".Underscore());
            p.Property(p => p.RatePerKg).HasColumnName($"r_{nameof(SellerManagedRate.RatePerKg)}".Underscore());
            p.Property(p => p.RatePerKm).HasColumnName($"r_{nameof(SellerManagedRate.RatePerKm)}".Underscore());
            p.Property(p => p.MinFee).HasColumnName($"r_{nameof(SellerManagedRate.MinFee)}".Underscore());
        });

        builder.ToTable(nameof(ShippingProfile).Underscore());
    }
}
