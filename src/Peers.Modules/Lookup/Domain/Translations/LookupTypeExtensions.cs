using Peers.Modules.Catalog.Domain.Translations;
using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Lookup.Domain.Translations;

public static class LookupTypeExtensions
{
    extension(LookupType value)
    {
        public void UpsertTranslations([NotNull] params (string lang, string name)[] trs)
        {
            var dtos = new LookupTypeTr.Dto[trs.Length];
            for (var i = 0; i < trs.Length; i++)
            {
                var (lang, name) = trs[i];
                dtos[i] = LookupTypeTr.Dto.Create(lang, name);
            }

            value.UpsertTranslations(dtos);
        }
    }
}
