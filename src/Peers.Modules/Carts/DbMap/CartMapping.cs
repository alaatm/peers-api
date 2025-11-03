using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Carts.Domain;

namespace Peers.Modules.Carts.DbMap;

internal sealed class CartMapping : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder
            .HasIndex(p => new { p.BuyerId, p.SellerId })
            .IsUnique();

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder
            .HasOne(p => p.Buyer)
            .WithMany()
            .HasForeignKey(p => p.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.Seller)
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(p => p.Lines, nav =>
        {
            nav
                .WithOwner(p => p.Cart)
                .HasForeignKey(p => p.CartId);

            // Ensure that each variant appears only once per cart
            nav
                .HasIndex(p => new { p.CartId, p.VariantId })
                .IsUnique();

            nav
                .HasOne(p => p.Listing)
                .WithMany()
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            nav
                .HasOne(p => p.Variant)
                .WithMany()
                .HasForeignKey(p => p.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            nav.ToTable(nameof(CartLine).Underscore());
        });

        builder.Navigation(p => p.Lines).AutoInclude(false);
        builder.ToTable(nameof(Cart).Underscore());
    }
}
