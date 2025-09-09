using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.Communication.Email.Configuration;

/// <summary>
/// The Email options object.
/// </summary>
public sealed class EmailConfig : IConfigSection
{
    public const string ConfigSection = "Email";
    static string IConfigSection.ConfigSection => ConfigSection;

    public string Host { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string SenderName { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public bool Enabled { get; set; }
}

internal sealed class EmailConfigValidator : IValidateOptions<EmailConfig>
{
    public ValidateOptionsResult Validate(string? name, EmailConfig options)
    {
        if (options.Host is null || !RegexStatic.SmtpUriRegex().IsMatch(options.Host))
        {
            return ValidateOptionsResult.Fail($"{EmailConfig.ConfigSection}:{nameof(EmailConfig.Host)} must be a valid SMTP host URI.");
        }
        if (string.IsNullOrEmpty(options.Username?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{EmailConfig.ConfigSection}:{nameof(EmailConfig.Username)} must not be empty.");
        }
        if (string.IsNullOrEmpty(options.Password?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{EmailConfig.ConfigSection}:{nameof(EmailConfig.Password)} must not be empty.");
        }
        if (string.IsNullOrEmpty(options.SenderName?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{EmailConfig.ConfigSection}:{nameof(EmailConfig.SenderName)} must not be empty.");
        }
        if (options.SenderEmail is null || !RegexStatic.EmailRegex().IsMatch(options.SenderEmail))
        {
            return ValidateOptionsResult.Fail($"{EmailConfig.ConfigSection}:{nameof(EmailConfig.SenderEmail)} must be a valid email address.");
        }
        if (options.Port == 0)
        {
            return ValidateOptionsResult.Fail($"{EmailConfig.ConfigSection}:{nameof(EmailConfig.Port)} must not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}
