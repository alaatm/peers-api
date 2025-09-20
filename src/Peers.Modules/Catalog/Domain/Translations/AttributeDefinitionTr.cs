using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Catalog.Domain.Translations;

public sealed class AttributeDefinitionTr : TranslationBase<AttributeDefinition, AttributeDefinitionTr>
{
    public string Name { get; set; } = default!;
    public string? Unit { get; set; }

    public sealed class Dto : DtoBase
    {
        public string Name { get; set; } = default!;
        public string? Unit { get; set; }

        public override void ApplyTo([NotNull] AttributeDefinitionTr target) => (target.Name, target.Unit) = (Name.Trim(), Unit?.Trim());
        public override void ApplyFrom([NotNull] AttributeDefinitionTr source) => (Name, Unit) = (source.Name, source.Unit);
        public static Dto Create(string langCode, string name, string? unit) => new() { LangCode = langCode, Name = name, Unit = unit };
    }
}
