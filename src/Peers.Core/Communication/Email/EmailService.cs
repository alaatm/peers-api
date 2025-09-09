using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Peers.Core.Communication.Email.Configuration;

namespace Peers.Core.Communication.Email;

/// <summary>
/// SMS service.
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly ISmtpClient _smtp;
    private readonly EmailConfig _config;
    private readonly ILogger<EmailService> _log;

    public EmailService(
        ISmtpClient smtp,
        EmailConfig config,
        ILogger<EmailService> log)
    {
        _smtp = smtp;
        _config = config;
        _log = log;
    }

    /// <summary>
    /// Asynchronously sends the email from the support account.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <param name="body">The body.</param>
    /// <param name="recipient">The recipient.</param>
    public async Task SendAsync(string subject, string body, string recipient)
    {
        if (!_config.Enabled)
        {
            _log.EmailServiceDisabled();
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);

        if (!RegexStatic.EmailRegex().IsMatch(recipient))
        {
            throw new ArgumentException("Recipient email must be a valid email address.", nameof(recipient));
        }

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config.SenderName, _config.SenderEmail));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = body };

        try
        {
            await _smtp.ConnectAsync(_config.Host, _config.Port, _config.EnableSsl);

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            _smtp.AuthenticationMechanisms.Remove("XOAUTH2");

            // Note: only needed if the SMTP server requires authentication
            await _smtp.AuthenticateAsync(_config.Username, _config.Password);
            await _smtp.SendAsync(message);
        }
        catch (Exception ex) when (ex is
            AuthenticationException or
            ProtocolException or
            ServiceNotConnectedException or
            ServiceNotAuthenticatedException or
            SmtpCommandException)
        {
            _log.EmailSendError(ex);
        }
        finally
        {
            await _smtp.DisconnectAsync(true);
        }
    }
}
