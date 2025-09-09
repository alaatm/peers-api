using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.DbMap;

internal sealed class NotificationMapping : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(p => p.Contents).Metadata.RemoveAnnotation("MaxLength");
        builder.ToTable(nameof(Notification).Underscore());
    }
}
