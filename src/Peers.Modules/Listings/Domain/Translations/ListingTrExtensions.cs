using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Listings.Domain.Translations;

public static class ListingTrExtensions
{
    extension(Listing listing)
    {
        public void UpsertTranslations([NotNull] params (string lang, string title, string? descr)[] trs)
        {
            var dtos = new ListingTr.Dto[trs.Length];
            for (var i = 0; i < trs.Length; i++)
            {
                var (lang, title, descr) = trs[i];
                dtos[i] = ListingTr.Dto.Create(lang, title, descr);
            }

            listing.UpsertTranslations(dtos);
        }
    }
}
