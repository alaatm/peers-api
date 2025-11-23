using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Customers.Domain;

namespace Peers.Modules.Customers.DbMap;

internal sealed class PaymentCardMapping : IEntityTypeConfiguration<PaymentCard>
{
    public void Configure(EntityTypeBuilder<PaymentCard> builder)
    {
        builder.HasIndex(p => p.PaymentId);
        builder.HasIndex(p => p.IsVerified);
        builder.Property(p => p.First6Digits).HasMaxLength(6);
        builder.Property(p => p.Last4Digits).HasMaxLength(4);
    }
}
