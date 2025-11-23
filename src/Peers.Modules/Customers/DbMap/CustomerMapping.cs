using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Customers.Domain;

namespace Peers.Modules.Customers.DbMap;

internal sealed class CustomerMapping : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasIndex(p => p.Username).IsUnique();
        builder.Property(p => p.Username).HasMaxLength(128);

        builder
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Customer>(p => p.Id)
            .IsRequired();

        builder
            .HasMany(p => p.PaymentMethods)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ToTable(nameof(Customer).Underscore());
    }
}
