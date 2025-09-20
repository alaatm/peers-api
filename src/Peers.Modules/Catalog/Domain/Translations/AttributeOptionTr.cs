using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Catalog.Domain.Translations;

public sealed class AttributeOptionTr : TranslationBase<AttributeOption, AttributeOptionTr>
{
    public string Name { get; set; } = default!;

    public sealed class Dto : DtoBase
    {
        public string Name { get; set; } = default!;

        public override void ApplyTo([NotNull] AttributeOptionTr target) => target.Name = Name.Trim();
        public override void ApplyFrom([NotNull] AttributeOptionTr source) => Name = source.Name;
        public static Dto Create(string langCode, string name) => new() { LangCode = langCode, Name = name };
    }
}
