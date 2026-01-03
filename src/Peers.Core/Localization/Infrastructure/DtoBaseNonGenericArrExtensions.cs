namespace Peers.Core.Localization.Infrastructure;

public static class DtoBaseNonGenericArrExtensions
{
    /// <summary>
    /// Gets the English translation from the array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="translations"></param>
    public static T? GetEn<T>(this T[] translations)
        where T : DtoBaseNonGeneric => translations.FirstOrDefault(t => t.LangCode.Equals(Lang.EnLangCode, StringComparison.OrdinalIgnoreCase));
}
