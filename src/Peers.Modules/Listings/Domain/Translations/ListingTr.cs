using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Listings.Domain.Translations;

public sealed class ListingTr : TranslationBase<Listing, ListingTr>
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public sealed class Dto : DtoBase
    {
        /// <summary>
        /// The localized listing title.
        /// </summary>
        public string Title { get; set; } = default!;
        /// <summary>
        /// The localized listing description.
        /// </summary>
        public string? Description { get; set; }

        public override void ApplyTo([NotNull] ListingTr target) => (target.Title, target.Description) = (Title.Trim(), Description?.Trim());
        public override void ApplyFrom([NotNull] ListingTr source) => (Title, Description) = (source.Title, source.Description);
        public static Dto Create(string langCode, string title, string? descr) => new() { LangCode = langCode, Title = title, Description = descr };
    }
}
