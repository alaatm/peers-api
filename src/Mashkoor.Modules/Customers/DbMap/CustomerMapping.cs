using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Modules.Customers.Domain;

namespace Mashkoor.Modules.Customers.DbMap;

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

        builder.ToTable(nameof(Customer).Underscore());
    }
}
