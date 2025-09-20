using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Catalog.Domain.Translations;

public static class AttributeDefinitionTrExtensions
{
    extension(AttributeDefinition def)
    {
        public void UpsertTranslations([NotNull] params (string lang, string name, string? unit)[] trs)
        {
            var dtos = new AttributeDefinitionTr.Dto[trs.Length];
            for (var i = 0; i < trs.Length; i++)
            {
                var (lang, name, unit) = trs[i];
                dtos[i] = AttributeDefinitionTr.Dto.Create(lang, name, unit);
            }

            def.UpsertTranslations(dtos);
        }
    }
}
