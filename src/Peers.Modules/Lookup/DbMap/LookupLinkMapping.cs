using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Lookup.DbMap;

internal sealed class LookupLinkMapping : IEntityTypeConfiguration<LookupLink>
{
    public void Configure(EntityTypeBuilder<LookupLink> builder)
    {
        builder.HasKey(p => new { p.ParentTypeId, p.ParentOptionId, p.ChildTypeId, p.ChildOptionId });

        builder
            .HasOne(p => p.ParentOption)
            .WithMany()
            .HasForeignKey(p => new { p.ParentTypeId, p.ParentOptionId })
            .HasPrincipalKey(p => new { p.TypeId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.ChildOption)
            .WithMany()
            .HasForeignKey(p => new { p.ChildTypeId, p.ChildOptionId })
            .HasPrincipalKey(p => new { p.TypeId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.ParentType)
            .WithMany(p => p.ParentLinks)
            .HasForeignKey(p => p.ParentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.ChildType)
            .WithMany(p => p.ChildLinks)
            .HasForeignKey(p => p.ChildTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(nameof(LookupLink).Underscore(), "lookup");
    }
}
