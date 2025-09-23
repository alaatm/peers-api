using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.I18n.DbMap;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Listings.Domain.Translations;

namespace Peers.Modules.Listings.DbMap;

internal sealed class ListingTrMapping : TranslationBaseMapping<Listing, ListingTr>
{
    protected override void ConfigureCore(EntityTypeBuilder<ListingTr> builder)
        => builder.ToTable(nameof(ListingTr).Underscore(), "i18n");
}
