using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.Communication.Sms.Configuration;

/// <summary>
/// The SMS options object.
/// </summary>
public sealed class SmsConfig : IConfigSection
{
    public const string ConfigSection = "Sms";
    static string IConfigSection.ConfigSection => ConfigSection;

    public string Sender { get; set; } = default!;
    public string Key { get; set; } = default!;
    public bool Enabled { get; set; }
}

internal sealed class SmsConfigValidator : IValidateOptions<SmsConfig>
{
    public ValidateOptionsResult Validate(string? name, SmsConfig options)
    {
        if (string.IsNullOrEmpty(options.Sender?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{SmsConfig.ConfigSection}:{nameof(SmsConfig.Sender)} must not be empty.");
        }
        if (string.IsNullOrEmpty(options.Key?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{SmsConfig.ConfigSection}:{nameof(SmsConfig.Key)} must not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}
