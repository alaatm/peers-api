using System.Text.Json;
using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.DbMap;

internal sealed class DeviceErrorMapping : IEntityTypeConfiguration<DeviceError>
{
    public void Configure(EntityTypeBuilder<DeviceError> builder)
    {
        builder.Property(p => p.AppVersion).HasMaxLength(64);
        builder.Property(p => p.AppState).HasMaxLength(64);
        builder.Property(p => p.Locale).HasMaxLength(12);
        builder.Property(p => p.Username).HasMaxLength(64);

        builder.Property(p => p.Exception).Metadata.RemoveAnnotation("MaxLength");
        builder.Property(p => p.DeviceInfo).Metadata.RemoveAnnotation("MaxLength");

        builder
            .Property(p => p.StackTrace)
            .HasConversion(
                p => JsonSerializer.Serialize(p, GlobalJsonOptions.Default),
                p => JsonSerializer.Deserialize<string[]>(p, GlobalJsonOptions.Default)!)
            .Metadata.SetValueComparer(ValueComparers.StringArrayComparer);

        builder
            .Property(p => p.Info)
            .HasConversion(
                p => JsonSerializer.Serialize(p, GlobalJsonOptions.Default),
                p => JsonSerializer.Deserialize<string[]>(p, GlobalJsonOptions.Default)!)
            .Metadata.SetValueComparer(ValueComparers.StringArrayComparer);

        builder.ToTable(nameof(DeviceError).Underscore(), "dbo");
    }
}
