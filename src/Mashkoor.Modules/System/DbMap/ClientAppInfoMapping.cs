using Humanizer;
using Mashkoor.Modules;
using Mashkoor.Modules.System.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mashkoor.Modules.System.DbMap;

internal sealed class ClientAppInfoMapping : IEntityTypeConfiguration<ClientAppInfo>
{
    public void Configure(EntityTypeBuilder<ClientAppInfo> builder)
    {
        builder
            .Property(e => e.Id)
            .HasDefaultValue(1);

        builder.HasIndex(p => p.PackageName).IsUnique();
        builder.HasIndex(p => p.HashString).IsUnique();
        builder.HasIndex(p => p.AndroidStoreLink).IsUnique();
        builder.HasIndex(p => p.IOSStoreLink).IsUnique();

        builder.Property(p => p.PackageName).HasMaxLength(64);
        builder.Property(p => p.HashString).HasMaxLength(64);
        builder.Property(p => p.AndroidStoreLink).HasMaxLength(128);
        builder.Property(p => p.IOSStoreLink).HasMaxLength(128);
        builder.ComplexProperty(p => p.LatestVersion);

        builder.ToTable(nameof(ClientAppInfo).Underscore());
    }
}
