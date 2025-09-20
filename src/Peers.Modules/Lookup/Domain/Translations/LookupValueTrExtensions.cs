using Peers.Modules.Catalog.Domain.Translations;
using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Lookup.Domain.Translations;

public static class LookupValueTrExtensions
{
    extension(LookupValue value)
    {
        public void UpsertTranslations([NotNull] params (string lang, string name)[] trs)
        {
            var dtos = new LookupValueTr.Dto[trs.Length];
            for (var i = 0; i < trs.Length; i++)
            {
                var (lang, name) = trs[i];
                dtos[i] = LookupValueTr.Dto.Create(lang, name);
            }

            value.UpsertTranslations(dtos);
        }
    }
}
