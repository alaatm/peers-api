using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Catalog.Domain.Translations;

public static class ProductTypeTrExtensions
{
    extension(ProductType pt)
    {
        public void UpsertTranslations([NotNull] params (string lang, string name)[] trs)
        {
            var dtos = new ProductTypeTr.Dto[trs.Length];
            for (var i = 0; i < trs.Length; i++)
            {
                var (lang, name) = trs[i];
                dtos[i] = ProductTypeTr.Dto.Create(lang, name);
            }

            pt.UpsertTranslations(dtos);
        }
    }
}
