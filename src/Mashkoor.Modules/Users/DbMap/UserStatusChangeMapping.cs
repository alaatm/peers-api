using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Partners.DbMap;

internal sealed class UserStatusChangeMapping : IEntityTypeConfiguration<UserStatusChange>
{
    public void Configure(EntityTypeBuilder<UserStatusChange> builder)
    {
        builder.Property(p => p.ChangeReason).HasMaxLength(1000);

        builder
            .HasOne(p => p.ChangedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.ToTable("user_status_change_history");
    }
}
