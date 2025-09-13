using Peers.Core.Domain;

namespace Peers.Core.Localization.Infrastructure;

/// <summary>
/// Defines a contract for an entity translation with language-specific data.
/// </summary>
/// <typeparam name="T">The type of the entity associated with the translation. Must implement <see cref="IEntity"/>.</typeparam>
public interface ITranslation<T>
    where T : IEntity
{
    /// <summary>
    /// The entity id.
    /// </summary>
    int EntityId { get; }
    /// <summary>
    /// The ISO 639-1 language code associated with the content.
    /// </summary>
    string LangCode { get; init; }
}
