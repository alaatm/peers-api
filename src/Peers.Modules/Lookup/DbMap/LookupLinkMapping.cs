using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Lookup.DbMap;

internal sealed class LookupLinkMapping : IEntityTypeConfiguration<LookupLink>
{
    public void Configure(EntityTypeBuilder<LookupLink> builder)
    {
        builder.HasKey(p => new { p.ParentTypeId, p.ParentValueId, p.ChildTypeId, p.ChildValueId });

        builder
            .HasOne(p => p.ParentValue)
            .WithMany()
            .HasForeignKey(p => new { p.ParentTypeId, p.ParentValueId })
            .HasPrincipalKey(p => new { p.TypeId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.ChildValue)
            .WithMany()
            .HasForeignKey(p => new { p.ChildTypeId, p.ChildValueId })
            .HasPrincipalKey(p => new { p.TypeId, p.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(nameof(LookupLink).Underscore(), "lookup");
    }
}
