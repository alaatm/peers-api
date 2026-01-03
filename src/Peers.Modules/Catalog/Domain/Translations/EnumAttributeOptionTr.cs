using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Catalog.Domain.Translations;

public sealed class EnumAttributeOptionTr : TranslationBase<EnumAttributeOption, EnumAttributeOptionTr>
{
    public string Name { get; set; } = default!;

    public sealed class Dto : DtoBase
    {
        /// <summary>
        /// The localized name of the enum attribute option.
        /// </summary>
        public string Name { get; set; } = default!;

        public override void ApplyTo([NotNull] EnumAttributeOptionTr target) => target.Name = Name.Trim();
        public override void ApplyFrom([NotNull] EnumAttributeOptionTr source) => Name = source.Name;
        public static Dto Create(string langCode, string name) => new() { LangCode = langCode, Name = name };
    }
}
