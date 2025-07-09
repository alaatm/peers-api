using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.DbMap;

internal sealed class NotificationMapping : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(p => p.Contents).Metadata.RemoveAnnotation("MaxLength");
        builder.ToTable(nameof(Notification).Underscore());
    }
}
