using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Settings.Domain;

/// <summary>
/// Translations for <see cref="Terms"/> entity.
/// </summary>
public sealed class TermsTr : TranslationBase<Terms, TermsTr>
{
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;

    public sealed class Dto : DtoBase
    {
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;

        public override void ApplyTo([NotNull] TermsTr target) => (target.Title, target.Body) = (Title.Trim(), Body.Trim());
        public override void ApplyFrom([NotNull] TermsTr source) => (Title, Body) = (source.Title, source.Body);
        public static Dto Create(string langCode, string title, string body) => new() { LangCode = langCode, Title = title, Body = body };
    }
}
