using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.DbMap;

internal sealed class UserNotificationMapping : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder
            .HasOne(p => p.User)
            .WithMany(p => p.Notifications)
            .HasForeignKey("user_id")
            .IsRequired();

        builder
            .HasOne(p => p.Notification)
            .WithMany()
            .HasForeignKey("notification_id")
            .IsRequired();

        builder.ToTable(nameof(UserNotification).Underscore());
    }
}
