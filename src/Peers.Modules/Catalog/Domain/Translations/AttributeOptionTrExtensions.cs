using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Catalog.Domain.Translations;

public static class AttributeOptionTrExtensions
{
    extension(AttributeOption opt)
    {
        public void UpsertTranslations([NotNull] params (string lang, string name)[] trs)
        {
            var dtos = new AttributeOptionTr.Dto[trs.Length];
            for (var i = 0; i < trs.Length; i++)
            {
                var (lang, name) = trs[i];
                dtos[i] = AttributeOptionTr.Dto.Create(lang, name);
            }

            opt.UpsertTranslations(dtos);
        }
    }
}
