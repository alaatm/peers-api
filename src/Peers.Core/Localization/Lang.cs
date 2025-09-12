using System.Diagnostics;

namespace Peers.Core.Localization;

/// <summary>
/// Language localization helper.
/// </summary>
public static class Lang
{
    /// <summary>
    /// IMPORTANT: Must ALWAYS be in sync with languages defined in Peers.Modules.I18n.Domain.Language.SupportedLanguages.
    /// The list of supported languages.
    /// </summary>
    public static readonly string[] SupportedLanguages =
    [
        EnLangCode,
        ArLangCode,
    ];

    public const string EnLangCode = "en";
    public const string ArLangCode = "ar";
    public const string DefaultLangCode = EnLangCode;

    /// <summary>
    /// Returns the current thread language. Defaults to <see cref="DefaultLangCode" /> if the current language is
    /// not part of the supported languages.
    /// </summary>
    /// <returns></returns>
    public static string GetCurrent() => GetOrDefault(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);

    /// <summary>
    /// Returns the requested language if supported; otherwise returns the default language.
    /// </summary>
    /// <param name="requestedLang">The requested language.</param>
    /// <returns></returns>
    public static string GetOrDefault(string? requestedLang)
    {
        if (requestedLang is null)
        {
            return DefaultLangCode;
        }

        Debug.Assert(requestedLang.Length == 2);

        foreach (var lang in SupportedLanguages)
        {
            if (requestedLang.Equals(lang, StringComparison.OrdinalIgnoreCase))
            {
                return lang;
            }
        }

        return DefaultLangCode;
    }
}
