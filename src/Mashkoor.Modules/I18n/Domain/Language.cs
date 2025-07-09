using System.ComponentModel.DataAnnotations.Schema;
using Mashkoor.Core.Localization;

namespace Mashkoor.Modules.I18n.Domain;

/// <summary>
/// Represents a support language where translations can be set for localizable entities.
/// </summary>
public sealed class Language
{
    public static Language Ar => new() { Id = Lang.ArLangCode, Name = "العربية", Dir = "rtl" };
    public static Language En => new() { Id = Lang.EnLangCode, Name = "English", Dir = "ltr" };
    public static Language Ru => new() { Id = Lang.RuLangCode, Name = "Русский", Dir = "ltr" };

    /// <summary>
    /// IMPORTANT: Must ALWAYS be in sync with <see cref="Lang.SupportedLanguages"/>.
    /// The list of supported languages.
    /// </summary>
    public static Language[] SupportedLanguages => [Ar, En, Ru];

    /// <summary>
    /// The language code.
    /// </summary>
    public string Id { get; set; } = default!;
    /// <summary>
    /// The language name.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// The language direction (LTR or RTL).
    /// </summary>
    [NotMapped]
    public string Dir { get; set; } = default!;

    /// <summary>
    /// Checks whether given translated field fulfills all required languages as specified.
    /// An error is returned if there are missing translations or extra translations.
    /// </summary>
    /// <param name="languages">The list of supported languages.</param>
    /// <param name="field">The translated field.</param>
    /// <param name="error">The error message.</param>
    /// <param name="args">The arguments to format the error message with.</param>
    /// <returns>True if has errors; otherwise, false.</returns>
    public static bool HasError(
        Language[] languages,
        LocalizedField[] field,
        [NotNullWhen(true)] out string? error,
        [NotNullWhen(true)] out string? args)
    {
        var missingLanguages = languages.Select(p => p.Id).Except(field.Select(p => p.Language));
        var extraLanguages = field.Select(p => p.Language).Except(languages.Select(p => p.Id));

        if (missingLanguages.Any())
        {
            error = "No translation was provided for the following language(s): {0}. Field name: '{1}'.";
            args = string.Join(',', missingLanguages);
            return true;
        }

        if (extraLanguages.Any())
        {
            error = "Translation was provided for the following non-supported language(s): {0}. Field name: '{1}'.";
            args = string.Join(',', extraLanguages);
            return true;
        }

        error = args = null;
        return false;
    }
}
