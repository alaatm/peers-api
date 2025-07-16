namespace Mashkoor.Core.Communication.Sms;

/// <summary>
/// SMS service contract.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Asynchronously sends an SMS to a recipient.
    /// </summary>
    /// <param name="recipient">The recipient.</param>
    /// <param name="body">The message body.</param>
    Task SendAsync(string recipient, string body);
}
