using Peers.Core.Domain;

namespace Peers.Core.Localization.Infrastructure;

/// <summary>
/// Provides a base class for translation entities associated with a specific entity type and language code.
/// </summary>
/// <typeparam name="T">The type of entity that this translation is associated with.</typeparam>
/// <typeparam name="TTr">The type of the translation entity.</typeparam>
public abstract class TranslationBase<T, TTr> : ITranslation<T>
    where T : IEntity
    where TTr : ITranslation<T>
{
    /// <summary>
    /// The entity id.
    /// </summary>
    public int EntityId { get; private set; }
    /// <summary>
    /// The ISO 639-1 language code associated with the content.
    /// </summary>
    public string LangCode { get; init; } = default!;

    public abstract class DtoBase
        : IIncomingTranslation<TTr>,
          IOutgoingTranslation<TTr>
    {
        public string LangCode { get; set; } = default!;

        // write → entity
        public abstract void ApplyTo(TTr target);

        // read ← entity
        public abstract void ApplyFrom(TTr source);
    }
}
