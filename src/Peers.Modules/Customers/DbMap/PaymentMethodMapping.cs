using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Customers.Domain;

namespace Peers.Modules.Customers.DbMap;

internal sealed class PaymentMethodMapping : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder
            .HasDiscriminator(p => p.Type)
            .HasValue<PaymentCard>(PaymentType.Card)
            .HasValue<ApplePay>(PaymentType.ApplePay)
            .HasValue<Cash>(PaymentType.Cash);

        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.IsDeleted);

        builder
            .Property(p => p.IsDeleted)
            .HasComputedColumnSql($"CAST(CASE WHEN [{nameof(PaymentMethod.DeletedOn).Underscore()}] IS NULL THEN 0 ELSE 1 END AS bit)");

        builder.ToTable(nameof(PaymentMethod).Underscore());
    }
}
