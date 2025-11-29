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

        builder.OwnsOne(p => p.Nafath, nav =>
        {
            nav.WithOwner().HasForeignKey(p => p.Id);
            nav.Property(p => p.NationalId).HasMaxLength(10);
            nav.Property(p => p.FirstNameAr).HasMaxLength(64);
            nav.Property(p => p.LastNameAr).HasMaxLength(64);
            nav.Property(p => p.FirstNameEn).HasMaxLength(64);
            nav.Property(p => p.LastNameEn).HasMaxLength(64);
            nav.Property(p => p.Gender).HasMaxLength(1);
            nav.ToTable(nameof(NafathInfo).Underscore());
        });

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

        builder.Navigation(p => p.Nafath).AutoInclude(false);
        builder.ToTable(nameof(Seller).Underscore());
    }
}
