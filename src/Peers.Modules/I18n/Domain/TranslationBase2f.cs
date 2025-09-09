namespace Peers.Modules.I18n.Domain;

/// <inheritdoc />
public class TranslationBase2f<T> : TranslationBase<T>
    where T : Entity
{
    /// <summary>
    /// The translation for the description field.
    /// </summary>
    public string Description { get; set; } = default!;
}
