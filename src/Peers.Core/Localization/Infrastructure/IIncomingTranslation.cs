namespace Peers.Core.Localization.Infrastructure;

/// <summary>
/// Defines a contract for applying an incoming translation to a target object.
/// </summary>
/// <typeparam name="TTr">The type of the target object to which the translation will be applied.</typeparam>
public interface IIncomingTranslation<TTr>
{
    /// <summary>
    /// The language code associated with the content.
    /// </summary>
    string LangCode { get; }
    /// <summary>
    /// Applies the current translation values to the target translation object.
    /// </summary>
    /// <param name="target">The target translation object to which the values will be applied.</param>
    void ApplyTo(TTr target);
}
