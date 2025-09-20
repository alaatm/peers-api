using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Catalog.Domain.Translations;

public sealed class ProductTypeTr : TranslationBase<ProductType, ProductTypeTr>
{
    public string Name { get; set; } = default!;

    public sealed class Dto : DtoBase
    {
        public string Name { get; set; } = default!;

        public override void ApplyTo([NotNull] ProductTypeTr target) => target.Name = Name.Trim();
        public override void ApplyFrom([NotNull] ProductTypeTr source) => Name = source.Name;
        public static Dto Create(string langCode, string name) => new() { LangCode = langCode, Name = name };
    }
}
