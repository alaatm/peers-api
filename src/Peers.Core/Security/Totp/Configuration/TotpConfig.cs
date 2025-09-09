using Peers.Core.Common.Configs;
using Microsoft.Extensions.Options;

namespace Peers.Core.Security.Totp.Configuration;

/// <summary>
/// The TOTP configuration.
/// </summary>
public sealed class TotpConfig : IConfigSection
{
    public const string ConfigSection = "totp";
    static string IConfigSection.ConfigSection => ConfigSection;

    /// <summary>
    /// Indicates whether to use the default OTP or not.
    /// </summary>
    public bool UseDefaultOtp { get; set; }
    /// <summary>
    /// The default OTP.
    /// </summary>
    public string DefaultOtp { get; set; } = default!;
    /// <summary>
    /// The duration of issued OTP in minutes.
    /// </summary>
    public TimeSpan Duration { get; set; }
}

internal sealed class TotpConfigValidator : IValidateOptions<TotpConfig>
{
    public ValidateOptionsResult Validate(string? name, TotpConfig options)
    {
        if (options.UseDefaultOtp && (options.DefaultOtp is null || !RegexStatic.OtpRegex().IsMatch(options.DefaultOtp)))
        {
            return ValidateOptionsResult.Fail($"{TotpConfig.ConfigSection}:{nameof(TotpConfig.DefaultOtp)} must be a 4 digit code.");
        }
        if (options.Duration.TotalSeconds < 3 * 60)
        {
            return ValidateOptionsResult.Fail($"{TotpConfig.ConfigSection}:{nameof(TotpConfig.Duration)} must be set to 3 minutes or greater.");
        }

        return ValidateOptionsResult.Success;
    }
}

