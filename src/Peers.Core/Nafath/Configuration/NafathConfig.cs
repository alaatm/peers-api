using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.Nafath.Configuration;

public sealed class NafathConfig : IConfigSection
{
    public const string ConfigSection = "Nafath";
    static string IConfigSection.ConfigSection => ConfigSection;

    public bool UseSandbox { get; set; } = true;
    public string AppId { get; set; } = default!;
    public string AppKey { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public bool Bypass { get; set; }
}

internal sealed class NafathConfigValidator : IValidateOptions<NafathConfig>
{
    public ValidateOptionsResult Validate(string? name, NafathConfig options)
    {
        if (string.IsNullOrEmpty(options.AppId?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{NafathConfig.ConfigSection}:{nameof(NafathConfig.AppId)} must not be empty.");
        }

        if (string.IsNullOrEmpty(options.AppKey?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{NafathConfig.ConfigSection}:{nameof(NafathConfig.AppKey)} must not be empty.");
        }

        if (string.IsNullOrEmpty(options.Issuer?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{NafathConfig.ConfigSection}:{nameof(NafathConfig.Issuer)} must not be empty.");
        }

        if (string.IsNullOrEmpty(options.Audience?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{NafathConfig.ConfigSection}:{nameof(NafathConfig.Audience)} must not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}

