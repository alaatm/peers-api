using System.Diagnostics;
using Mashkoor.Core.Localization;

namespace Mashkoor.Modules.I18n.Domain;

/// <summary>
/// Provides extension methods to work with translations.
/// </summary>
public static class LocalizedEntityExtensions
{
    /// <summary>
    /// Returns the localized value of the name field using the currently set language.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TTranslation">The type of the translation entity.</typeparam>
    /// <param name="te">The translatable entity.</param>
    /// <returns></returns>
    public static string LocalizedName<TEntity, TTranslation>(
        [NotNull] this ILocalizedEntity<TEntity, TTranslation> te)
        where TEntity : Entity
        where TTranslation : TranslationBase<TEntity>
        => te
            .Translations
            .SingleOrDefault(p =>
                p.LanguageId.Equals(Lang.GetCurrent(), StringComparison.OrdinalIgnoreCase))?.Name ?? "N/A";

    /// <summary>
    /// Returns the value of the name field in the requested language.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TTranslation">The type of the translation entity.</typeparam>
    /// <param name="te">The translatable entity.</param>
    /// <param name="lang">The requested language</param>
    /// <returns></returns>
    public static string NameForLang<TEntity, TTranslation>(
        [NotNull] this ILocalizedEntity<TEntity, TTranslation> te,
        string lang)
        where TEntity : Entity
        where TTranslation : TranslationBase<TEntity>
        => te
            .Translations
            .SingleOrDefault(p =>
                p.LanguageId.Equals(lang, StringComparison.OrdinalIgnoreCase))?.Name ?? "N/A";

    /// <summary>
    /// Adds or updates the translations for the name field of the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TTranslation">The type of the translation entity.</typeparam>
    /// <param name="te">The translatable entity.</param>
    /// <param name="name">The name field translations</param>
    public static void AddOrUpdateTranslations<TEntity, TTranslation>(
        [NotNull] this ILocalizedEntity<TEntity, TTranslation> te,
        [NotNull] IEnumerable<TranslatedField> name)
        where TEntity : Entity
        where TTranslation : TranslationBase<TEntity>
    {
        foreach (var nameEntry in name)
        {
            if (te
                .Translations
                .SingleOrDefault(p => p.LanguageId.Equals(
                    nameEntry.Language.Id,
                    StringComparison.OrdinalIgnoreCase)) is not TTranslation t)
            {
                t = Activator.CreateInstance<TTranslation>();
                t.Entity = (TEntity)te;
                t.LanguageId = nameEntry.Language.Id;
                te.Translations.Add(t);
            }

            t.Name = nameEntry.Value.Trim();
        }
    }

    /// <summary>
    /// Adds or updates the translations for the name and description fields of the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TTranslation">The type of the translation entity.</typeparam>
    /// <param name="te">The translatable entity.</param>
    /// <param name="name">The name field translations</param>
    /// <param name="description">The description field translations.</param>
    public static void AddOrUpdateTranslations<TEntity, TTranslation>(
        [NotNull] this ILocalizedEntity<TEntity, TTranslation> te,
        [NotNull] TranslatedField[] name,
        [NotNull] TranslatedField[] description)
        where TEntity : Entity
        where TTranslation : TranslationBase2f<TEntity>
    {
        Debug.Assert(name.Length == description.Length);

        for (var i = 0; i < name.Length; i++)
        {
            var nameEntry = name[i];
            var descriptionEntry = description[i];

            Debug.Assert(nameEntry.Language.Id.Equals(descriptionEntry.Language.Id, StringComparison.OrdinalIgnoreCase));

            if (te
                .Translations
                .SingleOrDefault(p => p.LanguageId.Equals(
                    nameEntry.Language.Id,
                    StringComparison.OrdinalIgnoreCase)) is not TTranslation t)
            {
                t = Activator.CreateInstance<TTranslation>();
                t.Entity = (TEntity)te;
                t.LanguageId = nameEntry.Language.Id;
                te.Translations.Add(t);
            }

            t.Name = nameEntry.Value.Trim();
            t.Description = descriptionEntry.Value.Trim();
        }
    }

    public static LocalizedField[] LocalizedNameFields<TEntity, TTranslation>(
        [NotNull] this ILocalizedEntity<TEntity, TTranslation> te)
        where TEntity : Entity
        where TTranslation : TranslationBase<TEntity>
    {
        var translations = te.Translations;
        var result = new LocalizedField[translations.Count];

        var i = 0;
        foreach (var translation in translations)
        {
            result[i++] = new LocalizedField(translation.LanguageId, translation.Name);
        }

        return result;
    }

    public static LocalizedField[] LocalizedNameFields2f<TEntity, TTranslation>(
        [NotNull] this ILocalizedEntity<TEntity, TTranslation> te)
        where TEntity : Entity
        where TTranslation : TranslationBase2f<TEntity>
    {
        var translations = te.Translations;
        var result = new LocalizedField[translations.Count];

        var i = 0;
        foreach (var translation in translations)
        {
            result[i++] = new LocalizedField(translation.LanguageId, translation.Name);
        }

        return result;
    }

    public static LocalizedField[] LocalizedDescrFields2f<TEntity, TTranslation>(
        [NotNull] this ILocalizedEntity<TEntity, TTranslation> te)
        where TEntity : Entity
        where TTranslation : TranslationBase2f<TEntity>
    {
        var translations = te.Translations;
        var result = new LocalizedField[translations.Count];

        var i = 0;
        foreach (var translation in translations)
        {
            result[i++] = new LocalizedField(translation.LanguageId, translation.Description);
        }

        return result;
    }
}
