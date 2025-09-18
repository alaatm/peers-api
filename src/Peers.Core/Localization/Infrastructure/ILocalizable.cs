using Peers.Core.Domain;

namespace Peers.Core.Localization.Infrastructure;

/// <summary>
/// Defines an entity that supports localization by associating it with a collection of translations.
/// </summary>
/// <typeparam name="T">The type of the entity being localized. Must implement <see cref="IEntity"/>.</typeparam>
/// <typeparam name="TTr">The type representing a translation for the entity. Must implement <see cref="ITranslation{T}"/>.</typeparam>
public interface ILocalizable<T, TTr>
    where T : IEntity
    where TTr : ITranslation<T>
{
    /// <summary>
    /// The collection of translations associated with the current entity.
    /// </summary>
    List<TTr> Translations { get; }
}
