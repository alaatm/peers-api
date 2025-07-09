namespace Mashkoor.Modules.I18n.Domain;

/// <summary>
/// Represents a translation entity.
/// </summary>
/// <typeparam name="T">The entity type that is translated.</typeparam>
public abstract class TranslationBase<T>
    where T : Entity
{
    /// <summary>
    /// The entity id.
    /// </summary>
    public int EntityId { get; set; }
    /// <summary>
    /// The language id.
    /// </summary>
    public string LanguageId { get; set; } = default!;
    /// <summary>
    /// The translation for the name field.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// The entity.
    /// </summary>
    public T Entity { get; set; } = default!;
}
