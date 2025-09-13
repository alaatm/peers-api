using System.Diagnostics.CodeAnalysis;
using Peers.Core.Domain;

namespace Peers.Core.Localization.Infrastructure;

/// <summary>
/// Provides extension methods for working with localizable entities and their translations.
/// </summary>
public static class LocalizableExtensions
{
    /// <summary>
    /// Retrieves the translation for the current language from the specified localizable entity.
    /// </summary>
    /// <remarks>This method selects the translation whose language code matches the current language as
    /// determined by Lang.GetCurrent(). If no matching translation exists, the method returns null.</remarks>
    /// <typeparam name="T">The type of the entity being localized.</typeparam>
    /// <typeparam name="TTr">The type of the translation associated with the entity.</typeparam>
    /// <param name="entity">The localizable entity from which to retrieve the translation. Cannot be null.</param>
    /// <returns>The translation for the current language if found; otherwise, null.</returns>
    public static TTr? Tr<T, TTr>(
        [NotNull] this ILocalizable<T, TTr> entity)
        where T : IEntity
        where TTr : ITranslation<T>
        => Tr(entity, Lang.GetCurrent());

    /// <summary>
    /// Retrieves the translation for the specified language code from the given localizable entity.
    /// </summary>
    /// <typeparam name="T">The type of the entity being localized.</typeparam>
    /// <typeparam name="TTr">The type of the translation associated with the entity.</typeparam>
    /// <param name="entity">The localizable entity from which to retrieve the translation. Cannot be null.</param>
    /// <param name="langCode">The language code identifying the desired translation.</param>
    /// <returns>The translation matching the specified language code, or <see langword="null"/> if no matching translation is
    /// found.</returns>
    public static TTr? Tr<T, TTr>(
        [NotNull] this ILocalizable<T, TTr> entity,
        string langCode)
        where T : IEntity
        where TTr : ITranslation<T>
        => entity
            .Translations
            .SingleOrDefault(p => p.LangCode == langCode);

    /// <summary>
    /// Adds new translations or updates existing translations for the specified entity based on the provided incoming
    /// translation data.
    /// </summary>
    /// <remarks>Each translation in <paramref name="incoming"/> is matched by language code to existing
    /// translations on the entity. If a translation for a language already exists, it is updated; otherwise, a new
    /// translation is added. All language codes must be supported and unique within the input array.</remarks>
    /// <typeparam name="T">The type of the entity being localized.</typeparam>
    /// <typeparam name="TTr">The type of the translation associated with the entity.</typeparam>
    /// <typeparam name="TIn">The type of the incoming translation data.</typeparam>
    /// <param name="entity">The localizable entity whose translations will be inserted or updated.</param>
    /// <param name="incoming">An array of incoming translation data to apply to the entity. Each element must specify a supported and unique
    /// language code.</param>
    /// <exception cref="TranslationValidationException">Thrown if an incoming translation specifies an unsupported or duplicate language code.</exception>
    public static void UpsertTranslations<T, TTr, TIn>(
        [NotNull] this ILocalizable<T, TTr> entity,
        [NotNull] TIn[] incoming)
        where T : IEntity
        where TTr : ITranslation<T>, new()
        where TIn : IIncomingTranslation<TTr>
    {
        var byLangCode = entity.Translations.ToDictionary(t => t.LangCode, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var input in incoming)
        {
            var langCode = input.LangCode;

            if (!Lang.SupportedLanguages.Contains(langCode, StringComparer.Ordinal))
            {
                throw new TranslationValidationException("Unsupported language '{0}'.", langCode);
            }
            if (!seen.Add(langCode))
            {
                throw new TranslationValidationException("Duplicate language '{0}'.", langCode);
            }

            if (byLangCode.TryGetValue(langCode, out var existing))
            {
                input.ApplyTo(existing);
            }
            else
            {
                var t = new TTr() { LangCode = langCode };
                input.ApplyTo(t);
                entity.Translations.Add(t);
            }
        }
    }

    /// <summary>
    /// Generates an array of outgoing translation objects for all supported languages based on the translations of the
    /// specified localizable entity and writes it to the <paramref name="result"/> parameter.
    /// </summary>
    /// <remarks>The returned array contains one element per supported language, ordered according to <see
    /// cref="Lang.SupportedLanguages"/>. Each element is initialized with its language code, and populated with
    /// translation data if available.</remarks>
    /// <typeparam name="T">The type of the entity being localized.</typeparam>
    /// <typeparam name="TTr">The type representing a translation of the entity.</typeparam>
    /// <typeparam name="TOut">The type of the outgoing translation data transfer object.</typeparam>
    /// <param name="entity">The localizable entity from which to generate outgoing translations</param>
    /// <param name="result">An output parameter that will contain an array of outgoing translation objects for all supported languages.</param>
    /// <returns>An array of outgoing translation objects, one for each supported language. If a translation does not exist for a
    /// language, the corresponding object will have default values.</returns>
    public static TOut[] WriteTranslations<T, TTr, TOut>(
        [NotNull] this ILocalizable<T, TTr> entity,
        out TOut[] result)
        where T : IEntity
        where TTr : ITranslation<T>
        where TOut : IOutgoingTranslation<TTr>, new()
    {
        var langs = Lang.SupportedLanguages;
        var byLangCode = entity.Translations.ToDictionary(t => t.LangCode, StringComparer.Ordinal);
        result = new TOut[langs.Length];

        for (var i = 0; i < langs.Length; i++)
        {
            var langCode = langs[i];
            var dto = new TOut { LangCode = langCode };
            if (byLangCode.TryGetValue(langCode, out var tr))
            {
                dto.ApplyFrom(tr);
            }

            // else: leave fields null/default
            result[i] = dto;
        }

        return result;
    }

    /// <summary>
    /// Generates a dictionary mapping language codes to outgoing translation objects for the specified localizable entity and writes it to the <paramref name="result"/> parameter.
    /// </summary>
    /// <remarks>The returned dictionary uses ordinal string comparison for language codes. This method is
    /// typically used to facilitate access to translations by language code.</remarks>
    /// <typeparam name="T">The type of the entity being localized.</typeparam>
    /// <typeparam name="TTr">The type representing a translation of the entity.</typeparam>
    /// <typeparam name="TOut">The type of the outgoing translation object.</typeparam>
    /// <param name="entity">The localizable entity for which to generate the translation map</param>
    /// <param name="result">An output parameter that will contain a dictionary mapping language codes to outgoing translation objects.</param>
    /// <returns>A dictionary where each key is a language code and each value is an outgoing translation object for that
    /// language. The dictionary is empty if the entity has no translations.</returns>
    public static void WriteTranslationMap<T, TTr, TOut>(
        [NotNull] this ILocalizable<T, TTr> entity,
        out Dictionary<string, TOut> result)
        where T : IEntity
        where TTr : ITranslation<T>
        where TOut : IOutgoingTranslation<TTr>, new()
    {
        entity.WriteTranslations(out TOut[] resultArr);
        result = resultArr.ToDictionary(x => x.LangCode, x => x, StringComparer.Ordinal);
    }
}
