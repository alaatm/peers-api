using System.ComponentModel.DataAnnotations.Schema;
using Peers.Core.Localization;

namespace Peers.Modules.I18n.Domain;

/// <summary>
/// Represents a support language where translations can be set for localizable entities.
/// </summary>
public sealed class Language
{
    public static Language Ar => new() { Id = Lang.ArLangCode, Name = "العربية", Dir = "rtl" };
    public static Language En => new() { Id = Lang.EnLangCode, Name = "English", Dir = "ltr" };

    /// <summary>
    /// IMPORTANT: Must ALWAYS be in sync with <see cref="Lang.SupportedLanguages"/>.
    /// The list of supported languages.
    /// </summary>
    public static Language[] SupportedLanguages => [Ar, En];

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
}
