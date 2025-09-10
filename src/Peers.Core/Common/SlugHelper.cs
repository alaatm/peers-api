using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Peers.Core.Common;

public static partial class SlugHelper
{
    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex(@"-+", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex MultipleHyphensRegex();

    [GeneratedRegex(@"[\s/_|\\+&]+", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    private static partial Regex SeparatorsRegex();

    public static string ToSlug(string name)
    {
        const string Hyphen = "-";
        const char HyphenChar = '-';

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // Normalize to decompose accents (é -> e + ́)
        var slug = name
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        // Strip diacritic marks
        var sb = new StringBuilder(slug.Length);
        foreach (var c in slug)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        slug = sb.ToString();

        // Replace common separators with hyphen
        slug = SeparatorsRegex().Replace(slug, Hyphen);
        // Remove anything not a-z0-9 or hyphen
        slug = InvalidCharsRegex().Replace(slug, string.Empty);
        // Collapse multiple hyphens
        slug = MultipleHyphensRegex().Replace(slug, Hyphen);
        // Trim hyphens
        slug = slug.Trim(HyphenChar);

        return slug;
    }
}
