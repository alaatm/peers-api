using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Sellers.Domain;

namespace Peers.Modules.Sellers.DbMap;

internal sealed class SellerMapping : IEntityTypeConfiguration<Seller>
{
    public void Configure(EntityTypeBuilder<Seller> builder)
    {
        builder.HasIndex(p => p.Username).IsUnique();
        builder.Property(p => p.Username).HasMaxLength(128);

        builder
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Seller>(p => p.Id)
            .IsRequired();

        builder
            .HasMany(p => p.ShippingProfiles)
            .WithOne(p => p.Seller)
            .HasForeignKey(p => p.SellerId)
            .IsRequired();

        builder.ToTable(nameof(Seller).Underscore());
    }
}
