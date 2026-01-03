using System.Text.Json;
using Microsoft.Extensions.Options;
using Peers.Core.Common.Configs;

namespace Peers.Core.Communication.Push.Configuration;

public sealed class FirebaseConfig : IConfigSection
{
    public const string ConfigSection = "Firebase";
    static string IConfigSection.ConfigSection => ConfigSection;

    public string ServiceAccountKey { get; set; } = default!;
}

internal sealed class FirebaseConfigValidator : IValidateOptions<FirebaseConfig>
{
    public ValidateOptionsResult Validate(string? name, FirebaseConfig options)
    {
        try
        {
            var _ = JsonDocument.Parse(NormalizeMultilineStrings(options.ServiceAccountKey) ?? "");
        }
        catch (JsonException)
        {
            return ValidateOptionsResult.Fail($"{FirebaseConfig.ConfigSection}:{nameof(FirebaseConfig.ServiceAccountKey)} must be a valid JSON object.");
        }

        return ValidateOptionsResult.Success;
    }

    private static string NormalizeMultilineStrings(string json)
        => json.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
}

