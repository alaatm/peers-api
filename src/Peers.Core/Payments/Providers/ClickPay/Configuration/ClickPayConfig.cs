using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.Payments.Providers.ClickPay.Configuration;

/// <summary>
/// The ClickPay payment provider configuration.
/// </summary>
public sealed class ClickPayConfig : IConfigSection
{
    public const string ConfigSection = "ClickPayPaymentProvider";
    static string IConfigSection.ConfigSection => ConfigSection;

    public string ProfileId { get; set; } = default!;
    public string Key { get; set; } = default!;
    public string PayoutAccountId { get; set; } = default!;
}

internal sealed class ClickPayConfigValidator : IValidateOptions<ClickPayConfig>
{
    public ValidateOptionsResult Validate(string? name, ClickPayConfig options)
    {
        if (string.IsNullOrEmpty(options.ProfileId?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{ClickPayConfig.ConfigSection}:{nameof(ClickPayConfig.ProfileId)} must not be empty.");
        }
        if (string.IsNullOrEmpty(options.Key?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{ClickPayConfig.ConfigSection}:{nameof(ClickPayConfig.Key)} must not be empty.");
        }
        if (string.IsNullOrEmpty(options.PayoutAccountId?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{ClickPayConfig.ConfigSection}:{nameof(ClickPayConfig.PayoutAccountId)} must not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}
