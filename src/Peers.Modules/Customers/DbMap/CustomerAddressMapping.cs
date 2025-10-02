using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Customers.Domain;

namespace Peers.Modules.Customers.DbMap;

internal sealed class CustomerAddressMapping : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        var isDefaultColName = nameof(CustomerAddress.IsDefault).Underscore();

        builder.HasIndex(p => new { p.CustomerId, p.Name }).IsUnique();
        builder.HasIndex(p => p.CustomerId).HasFilter($"[{isDefaultColName}] = 1").IsUnique();
        builder.Property(p => p.Name).HasMaxLength(128);

        builder.ComplexProperty(p => p.Address);

        builder
            .HasOne(p => p.Customer)
            .WithMany(p => p.AddressList)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ToTable(nameof(CustomerAddress).Underscore());
    }
}
