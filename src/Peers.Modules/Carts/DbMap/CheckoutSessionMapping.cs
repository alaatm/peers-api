using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Ordering.Domain;

namespace Peers.Modules.Carts.DbMap;

internal sealed class CheckoutSessionMapping : IEntityTypeConfiguration<CheckoutSession>
{
    public void Configure(EntityTypeBuilder<CheckoutSession> builder)
    {
        builder
            .HasIndex(p => p.SessionId)
            .IsUnique();

        builder
            .HasIndex(p => p.PaymentId)
            .IsUnique();

        // Enforce single active/hpp_issued/paying session per cart
        builder
            .HasIndex(p => p.CartId)
            .IsUnique()
            .HasFilter($"[{nameof(CheckoutSession.Status).Underscore()}] IN ({(int)CheckoutSessionStatus.Active}, {(int)CheckoutSessionStatus.IntentIssued}, {(int)CheckoutSessionStatus.Paying})");

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder
            .HasOne(p => p.Cart)
            .WithMany(p => p.CheckoutSessions)
            .HasForeignKey(p => p.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(p => p.ShippingAddress)
            .WithMany()
            .HasForeignKey(p => p.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.PaymentMethod)
            .WithMany()
            .HasForeignKey(p => p.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.Order)
            .WithOne(p => p.CheckoutSession)
            .HasForeignKey<Order>(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(p => p.Lines, nav =>
        {
            nav
                .WithOwner(p => p.Session)
                .HasForeignKey(p => p.SessionId);

            // Ensure that each variant appears only once per session
            nav
                .HasIndex(p => new { p.SessionId, p.VariantId })
                .IsUnique();

            nav
                .HasOne(p => p.Listing)
                .WithMany()
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            nav
                .HasOne(p => p.Variant)
                .WithMany()
                .HasForeignKey(p => p.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            nav.ToTable(nameof(CheckoutSessionLine).Underscore());
        });

        builder.Navigation(p => p.Lines).AutoInclude(false);
        builder.ToTable(nameof(CheckoutSession).Underscore());
    }
}
