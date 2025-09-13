using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Settings.Domain;

/// <summary>
/// Represents value of privacy policy.
/// </summary>
public sealed class PrivacyPolicy : Entity, ILocalizable<PrivacyPolicy, PrivacyPolicyTr>
{
    /// <summary>
    /// The effective date of the policy.
    /// </summary>
    public DateOnly EffectiveDate { get; set; }
    /// <summary>
    /// The list of translations for this entity.
    /// </summary>
    public ICollection<PrivacyPolicyTr> Translations { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of <see cref="PrivacyPolicy"/>.
    /// </summary>
    /// <param name="effectiveDate">The effective date of the policy.</param>
    /// <param name="translations">The translations.</param>
    /// <returns></returns>
    public static PrivacyPolicy Create(
        DateOnly effectiveDate,
        [NotNull] PrivacyPolicyTr.Dto[] translations)
    {
        var privacyPolicy = new PrivacyPolicy
        {
            Translations = [],
            EffectiveDate = effectiveDate,
        };

        privacyPolicy.UpsertTranslations(translations);
        return privacyPolicy;
    }

    /// <summary>
    /// Updates privacy policy information.
    /// </summary>
    /// <param name="effectiveDate">The effective date of the policy.</param>
    /// <param name="translations">The translations.</param>
    public void Update(
        DateOnly effectiveDate,
        [NotNull] PrivacyPolicyTr.Dto[] translations)
    {
        EffectiveDate = effectiveDate;
        this.UpsertTranslations(translations);
    }
}
