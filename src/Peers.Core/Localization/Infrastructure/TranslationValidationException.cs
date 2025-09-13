using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Peers.Core.Localization.Infrastructure;

/// <summary>
/// Represents an exception that is thrown when a translation fails validation for a specific language.
/// </summary>
public sealed class TranslationValidationException : Exception
{
    /// <summary>
    /// The formatted representation of the error.
    /// </summary>
    public string Formatted { get; }
    /// <summary>
    /// The ISO language code associated with the content.
    /// </summary>
    public string LangCode { get; }

    public TranslationValidationException(string formatted, string langCode)
        : base(string.Format(CultureInfo.InvariantCulture, formatted, langCode))
    {
        Formatted = formatted;
        LangCode = langCode;
    }

    [ExcludeFromCodeCoverage]
    public TranslationValidationException() => throw new NotImplementedException();
    [ExcludeFromCodeCoverage]
    public TranslationValidationException(string message) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage]
    public TranslationValidationException(string message, Exception innerException) => throw new NotImplementedException();
}
