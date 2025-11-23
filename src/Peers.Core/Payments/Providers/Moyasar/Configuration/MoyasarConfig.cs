using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.Payments.Providers.Moyasar.Configuration;

/// <summary>
/// The Moyasar payment provider configuration.
/// </summary>
public sealed class MoyasarConfig : IConfigSection
{
    public const string ConfigSection = "MoyasarPaymentProvider";
    static string IConfigSection.ConfigSection => ConfigSection;

    public string PublishableKey { get; set; } = default!;
    public string Key { get; set; } = default!;
    public string PayoutAccountId { get; set; } = default!;
}

internal sealed class MoyasarConfigValidator : IValidateOptions<MoyasarConfig>
{
    public ValidateOptionsResult Validate(string? name, MoyasarConfig options)
    {
        if (string.IsNullOrEmpty(options.PublishableKey?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{MoyasarConfig.ConfigSection}:{nameof(MoyasarConfig.PublishableKey)} must not be empty.");
        }
        if (string.IsNullOrEmpty(options.Key?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{MoyasarConfig.ConfigSection}:{nameof(MoyasarConfig.Key)} must not be empty.");
        }
        if (string.IsNullOrEmpty(options.PayoutAccountId?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{MoyasarConfig.ConfigSection}:{nameof(MoyasarConfig.PayoutAccountId)} must not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}
