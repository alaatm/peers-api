using Microsoft.Extensions.Options;
using Mashkoor.Core.Common.Configs;

namespace Mashkoor.Core.AzureServices.Configuration;

/// <summary>
/// The Azure options object.
/// </summary>
public sealed class AzureConfig : IConfigSection
{
    public const string ConfigSection = "azure";
    static string IConfigSection.ConfigSection => ConfigSection;

    public string StorageConnectionString { get; set; } = default!;
    public Uri? DevTunnelsUri { get; set; }
}

internal sealed class AzureConfigValidator : IValidateOptions<AzureConfig>
{
    public ValidateOptionsResult Validate(string? name, AzureConfig options)
    {
        if (string.IsNullOrEmpty(options.StorageConnectionString?.Trim()))
        {
            return ValidateOptionsResult.Fail($"{AzureConfig.ConfigSection}:{nameof(AzureConfig.StorageConnectionString)} must not be empty.");
        }

        return ValidateOptionsResult.Success;
    }
}
