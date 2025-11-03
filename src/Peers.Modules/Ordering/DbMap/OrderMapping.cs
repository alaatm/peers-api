using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Ordering.Domain;

namespace Peers.Modules.Ordering.DbMap;

internal sealed class OrderMapping : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        var stateColName = nameof(Order.State).Underscore();

        builder
            .HasIndex(p => new { p.BuyerId, p.SellerId })
            .HasFilter($"[{stateColName}] = {(int)OrderState.Placed}")
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
                .WithOwner(p => p.Order)
                .HasForeignKey(p => p.OrderId);

            // Ensure that each variant appears only once per cart
            nav
                .HasIndex(p => new { p.OrderId, p.VariantId })
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

            nav.ToTable(nameof(OrderLine).Underscore());
        });

        builder.Navigation(p => p.Lines).AutoInclude(false);
        builder.ToTable(nameof(Order).Underscore());
    }
}
