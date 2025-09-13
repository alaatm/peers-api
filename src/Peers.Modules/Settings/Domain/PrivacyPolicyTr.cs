using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Settings.Domain;

/// <summary>
/// Translations for <see cref="PrivacyPolicy"/> entity.
/// </summary>
public sealed class PrivacyPolicyTr : TranslationBase<PrivacyPolicy, PrivacyPolicyTr>
{
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;

    public sealed class Dto : DtoBase
    {
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;

        public override void ApplyTo([NotNull] PrivacyPolicyTr target) => (target.Title, target.Body) = (Title.Trim(), Body.Trim());
        public override void ApplyFrom([NotNull] PrivacyPolicyTr source) => (Title, Body) = (source.Title, source.Body);
        public static Dto Create(string langCode, string title, string body) => new() { LangCode = langCode, Title = title, Body = body };
    }
}
