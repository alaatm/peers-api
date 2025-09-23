using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.DbMap;

internal sealed class AppUserMapping : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.UserName).IsUnique(true);
        builder.HasIndex(p => p.IsDeleted);

        builder.Property(p => p.PreferredLanguage).HasMaxLength(6);
        builder.Property(p => p.UserName).HasMaxLength(128);
        builder.Property(p => p.OriginalDeletedUsername).HasMaxLength(128);
        builder.Property(p => p.PhoneNumber).HasMaxLength(64);

        builder
            .HasMany(p => p.StatusChangeHistory)
            .WithOne()
            .HasForeignKey("user_id")
            .IsRequired();

        builder
            .OwnsMany(p => p.RefreshTokens, a =>
            {
                a.WithOwner().HasForeignKey("user_id");
                a.Property<int>("id");
                a.HasKey("id");
                a.HasIndex(p => p.Revoked);
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
                a.WithOwner().HasForeignKey("user_id");
                a.Property<int>("id");
                a.HasKey("id");
                a.ToTable(nameof(AppUsageHistory).Underscore(), "dbo");
            });

        builder.Navigation(p => p.RefreshTokens).AutoInclude(false);
        builder.Navigation(p => p.AppUsage).AutoInclude(false);
    }
}
