using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.DbMap;

internal sealed class DeviceMapping : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasIndex(p => p.DeviceId).IsUnique();
        builder.HasIndex(p => p.PnsHandle).IsUnique();

        builder.Property(p => p.App).HasMaxLength(64);
        builder.Property(p => p.AppVersion).HasMaxLength(64);
        builder.Property(p => p.DeviceType).HasMaxLength(64);
        builder.Property(p => p.Idiom).HasMaxLength(64);
        builder.Property(p => p.PnsHandle).HasMaxLength(400);

        builder.ToTable(nameof(Device).Underscore(), "dbo");
    }
}
