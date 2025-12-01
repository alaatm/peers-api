using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.DbMap;

internal sealed class AppUserMapping : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        const string IdColName = "id";
        const string UserIdColName = "user_id";

        builder.HasIndex(p => p.UserName).IsUnique();
        builder.HasIndex(p => p.PhoneNumber).IsUnique();

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsDeleted);

        builder.Property(p => p.UserName).IsRequired().HasMaxLength(128);
        builder.Property(p => p.PhoneNumber).HasMaxLength(64);

        builder.Property(p => p.PreferredLanguage).HasMaxLength(6);
        builder.Property(p => p.OriginalDeletedUsername).HasMaxLength(128);

        builder
            .HasMany(p => p.StatusChangeHistory)
            .WithOne()
            .HasForeignKey(UserIdColName)
            .IsRequired();

        builder
            .OwnsMany(p => p.RefreshTokens, a =>
            {
                var revokedColName = nameof(RefreshToken.Revoked).Underscore();

                a.WithOwner().HasForeignKey(UserIdColName);
                a.Property<int>(IdColName);
                a.HasKey(IdColName);
                a.HasIndex(UserIdColName).HasFilter($"[{revokedColName}] IS NULL").IsUnique(true);
                a.Property<byte[]>("concurrency_token").IsRowVersion();
                a.ToTable(nameof(RefreshToken).Underscore(), "dbo");
            });

        builder
            .HasMany(p => p.DeviceList)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);

        builder
            .OwnsMany(p => p.AppUsage, a =>
            {
                a.WithOwner().HasForeignKey(UserIdColName);
                a.Property<int>(IdColName);
                a.HasKey(IdColName);
                a.ToTable(nameof(AppUsageHistory).Underscore(), "dbo");
            });

        builder.Navigation(p => p.RefreshTokens).AutoInclude(false);
        builder.Navigation(p => p.AppUsage).AutoInclude(false);
    }
}
