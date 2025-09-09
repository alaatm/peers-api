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
        RuLangCode,
    ];

    public const string EnLangCode = "en";
    public const string ArLangCode = "ar";
    public const string RuLangCode = "ru";

    /// <summary>
    /// Returns the current thread language. Defaults to English if the current language is
    /// not part of the supported languages.
    /// </summary>
    /// <returns></returns>
    public static string GetCurrent() => GetOrDefault(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);

    /// <summary>
    /// Returns the requested language if found. Defaults to English if the requested language is
    /// not part of the supported languages.
    /// </summary>
    /// <param name="currentLang">The requested language.</param>
    /// <returns></returns>
    public static string GetOrDefault(string? currentLang)
    {
        if (currentLang is null)
        {
            return EnLangCode;
        }

        Debug.Assert(currentLang.Length == 2);

        foreach (var lang in SupportedLanguages)
        {
            if (currentLang.Equals(lang, StringComparison.OrdinalIgnoreCase))
            {
                return lang;
            }
        }

        return EnLangCode;
    }
}
