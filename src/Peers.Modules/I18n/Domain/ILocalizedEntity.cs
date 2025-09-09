namespace Peers.Modules.I18n.Domain;

/// <summary>
/// Represents an entity that supports translation of its fields.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TTranslation">The type of the translation entity.</typeparam>
public interface ILocalizedEntity<TEntity, TTranslation>
    where TEntity : Entity
    where TTranslation : TranslationBase<TEntity>
{
    /// <summary>
    /// The translations list for this entity.
    /// </summary>
    ICollection<TTranslation> Translations { get; set; }
}
