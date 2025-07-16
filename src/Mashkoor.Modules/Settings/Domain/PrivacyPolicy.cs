using Mashkoor.Modules.I18n.Domain;

namespace Mashkoor.Modules.Settings.Domain;

/// <summary>
/// Represents value of privacy policy.
/// </summary>
public sealed class PrivacyPolicy : Entity, ILocalizedEntity<PrivacyPolicy, PrivacyPolicyTranslation>
{
    /// <summary>
    /// The effective date of the policy.
    /// </summary>
    public DateOnly EffectiveDate { get; set; }
    /// <summary>
    /// The list of translations for this entity.
    /// </summary>
    public ICollection<PrivacyPolicyTranslation> Translations { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of <see cref="PrivacyPolicy"/>.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="body">The HTML body.</param>
    /// <param name="effectiveDate">The effective date of the policy.</param>
    /// <returns></returns>
    public static PrivacyPolicy Create(
        [NotNull] TranslatedField[] title,
        [NotNull] TranslatedField[] body,
        DateOnly effectiveDate)
    {
        var privacyPolicy = new PrivacyPolicy
        {
            Translations = [],
            EffectiveDate = effectiveDate,
        };

        privacyPolicy.AddOrUpdateTranslations(title, Normalize(body));
        return privacyPolicy;
    }

    /// <summary>
    /// Updates privacy policy information.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="body">The HTML body.</param>
    /// <param name="effectiveDate">The effective date of the policy.</param>
    public void Update(
        [NotNull] TranslatedField[] title,
        [NotNull] TranslatedField[] body,
        DateOnly effectiveDate)
    {
        EffectiveDate = effectiveDate;
        this.AddOrUpdateTranslations(title, Normalize(body));
    }

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
