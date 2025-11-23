using System.Text.Json.Serialization;

namespace Peers.Core.Payments.Providers.Moyasar.Models;

public class MoyasarUpdatePaymentRequest
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = default!;

    public static MoyasarUpdatePaymentRequest Create(string description, Dictionary<string, string> metadata) => new()
    {
        Description = description,
        Metadata = metadata,
    };
}
