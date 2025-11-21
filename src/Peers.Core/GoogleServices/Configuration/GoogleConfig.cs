using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.GoogleServices.Configuration;

public sealed class GoogleConfig : IConfigSection
{
    public const string ConfigSection = "Google";
    static string IConfigSection.ConfigSection => ConfigSection;

    public string ApiKey { get; set; } = default!;
}

internal sealed class GoogleConfigValidator : IValidateOptions<GoogleConfig>
{
    public ValidateOptionsResult Validate(string? name, GoogleConfig options)
    {
        if (string.IsNullOrEmpty(options.ApiKey?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{GoogleConfig.ConfigSection}:{nameof(GoogleConfig.ApiKey)} must not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}

