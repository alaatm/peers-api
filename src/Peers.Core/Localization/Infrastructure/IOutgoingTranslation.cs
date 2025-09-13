namespace Peers.Core.Localization.Infrastructure;

/// <summary>
/// Defines a contract for an outgoing translation that can be applied from a source object and associated with a
/// language code.
/// </summary>
/// <typeparam name="TTr">The type of the source object from which translation data is applied.</typeparam>
public interface IOutgoingTranslation<TTr>
{
    /// <summary>
    /// The language code associated with the content.
    /// </summary>
    string LangCode { get; set; }
    /// <summary>
    /// Applies values from the specified source translation object to the current instance.
    /// </summary>
    /// <param name="source">The source translation object from which values will be applied.</param>
    void ApplyFrom(TTr source);
}
