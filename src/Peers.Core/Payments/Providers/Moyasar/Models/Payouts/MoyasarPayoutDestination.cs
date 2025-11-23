using System.Text.Json.Serialization;

namespace Peers.Core.Payments.Providers.Moyasar.Models.Payouts;

public sealed class MoyasarPayoutDestination
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;
    [JsonPropertyName("iban")]
    public string Iban { get; set; } = default!;
    [JsonPropertyName("name")]
    public string BeneficiaryName { get; set; } = default!;
    [JsonPropertyName("mobile")]
    public string PhoneNumber { get; set; } = default!;
    [JsonPropertyName("country")]
    public string Country { get; set; } = default!;
    [JsonPropertyName("city")]
    public string City { get; set; } = default!;
}
