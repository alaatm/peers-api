using Humanizer;
using Peers.Modules.Media.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Peers.Modules.Media.DbMap;

internal sealed class MediaFileMapping : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.HasIndex(p => p.BatchId);
        builder.HasIndex(p => p.Approved);
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.EntityId);

        builder
            .HasIndex(p => new { p.Type, p.ThumbnailId, p.CustomerId })
            .IsUnique()
            .HasFilter($"[type] = {(int)MediaType.ProfilePicture}");

        builder
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .IsRequired(true);

        builder
            .HasOne(p => p.Thumbnail)
            .WithOne(p => p.Original)
            .HasForeignKey<MediaFile>(p => p.ThumbnailId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.ToTable(nameof(MediaFile).Underscore(), "dbo");
    }
}
