namespace Mashkoor.Core.Communication.Sms;

/// <summary>
/// Represents a service provider that facilitates sending SMS.
/// </summary>
public interface ISmsServiceProvider
{
    /// <summary>
    /// Asynchronously sends an SMS to a recipient.
    /// </summary>
    /// <param name="recipient">The recipient.</param>
    /// <param name="body">The message.</param>
    Task<TaqnyatResponse?> SendAsync(string recipient, string body);
}
