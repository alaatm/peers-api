using Peers.Core.Localization.Infrastructure;

namespace Peers.Modules.Settings.Domain;

/// <summary>
/// Represents value of terms and conditions.
/// </summary>
public sealed class Terms : Entity, ILocalizable<Terms, TermsTr>
{
    /// <summary>
    /// The list of translations for this entity.
    /// </summary>
    public ICollection<TermsTr> Translations { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of <see cref="Terms"/>.
    /// </summary>
    /// <param name="translations">The translations.</param>
    /// <returns></returns>
    public static Terms Create([NotNull] TermsTr.Dto[] translations)
    {
        var terms = new Terms
        {
            Translations = [],
        };

        terms.UpsertTranslations(translations);
        return terms;
    }

    /// <summary>
    /// Updates terms information.
    /// </summary>
    /// <param name="translations">The translations.</param>
    public void Update([NotNull] TermsTr.Dto[] translations)
        => this.UpsertTranslations(translations);
}
