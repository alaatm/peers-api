namespace Peers.Core.Communication.Email;

/// <summary>
/// SMS service contract.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Asynchronously sends the email from the support account.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <param name="body">The body.</param>
    /// <param name="recipient">The recipient.</param>
    Task SendAsync(
        string subject,
        string body,
        string recipient);
}
