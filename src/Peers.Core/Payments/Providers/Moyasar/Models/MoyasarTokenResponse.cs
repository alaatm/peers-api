using System.Globalization;
using System.Text.Json.Serialization;
using Peers.Core.Payments.Models;

namespace Peers.Core.Payments.Providers.Moyasar.Models;

/// <summary>
/// Represents a token response.
/// </summary>
public sealed class MoyasarTokenResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("brand")]
    public string Brand { get; set; } = default!;

    [JsonPropertyName("funding")]
    public string Funding { get; set; } = default!;

    [JsonPropertyName("country")]
    public string Country { get; set; } = default!;

    [JsonPropertyName("month")]
    public string Month { get; set; } = default!;

    [JsonPropertyName("year")]
    public string Year { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("last_four")]
    public string LastFour { get; set; } = default!;

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("verification_url")]
    public Uri? VerificationUrl { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public TokenResponse ToGeneric() => new()
    {
        CardBrand = PaymentCardUtils.ResolveCardBrand(Brand),
        CardType = PaymentCardUtils.ResolveCardFunding(Funding),
        MaskedCardNumber = LastFour,
        ExpiryMonth = int.Parse(Month, CultureInfo.InvariantCulture),
        ExpiryYear = int.Parse(Year, CultureInfo.InvariantCulture)
    };
}
