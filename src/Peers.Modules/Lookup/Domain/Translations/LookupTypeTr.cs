using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Lookup.Domain.Translations;

public sealed class LookupTypeTr : TranslationBase<LookupType, LookupTypeTr>
{
    public string Name { get; set; } = default!;

    public sealed class Dto : DtoBase
    {
        public string Name { get; set; } = default!;

        public override void ApplyTo([NotNull] LookupTypeTr target) => target.Name = Name.Trim();
        public override void ApplyFrom([NotNull] LookupTypeTr source) => Name = source.Name;
        public static Dto Create(string langCode, string name) => new() { LangCode = langCode, Name = name };
    }
}
