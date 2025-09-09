using Peers.Modules.I18n.Domain;

namespace Peers.Modules.Settings.Domain;

/// <summary>
/// Represents value of terms and conditions.
/// </summary>
public sealed class Terms : Entity, ILocalizedEntity<Terms, TermsTranslation>
{
    /// <summary>
    /// The list of translations for this entity.
    /// </summary>
    public ICollection<TermsTranslation> Translations { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of <see cref="Terms"/>.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="body">The HTML body.</param>
    /// <returns></returns>
    public static Terms Create(
        [NotNull] TranslatedField[] title,
        [NotNull] TranslatedField[] body)
    {
        var terms = new Terms
        {
            Translations = [],
        };

        terms.AddOrUpdateTranslations(title, Normalize(body));
        return terms;
    }

    /// <summary>
    /// Updates terms information.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="body">The HTML body.</param>
    public void Update(
        [NotNull] TranslatedField[] title,
        [NotNull] TranslatedField[] body)
        => this.AddOrUpdateTranslations(title, Normalize(body));

    private static TranslatedField[] Normalize(TranslatedField[] body)
    {
        for (var i = 0; i < body.Length; i++)
        {
            var field = body[i];

            var normalizedValue = field.Value
                .Replace("\r\n", "", StringComparison.Ordinal)
                .Replace("\n", "", StringComparison.Ordinal)
                .Replace("\r", "", StringComparison.Ordinal);

            body[i] = field with { Value = normalizedValue };
        }

        return body;
    }
}
