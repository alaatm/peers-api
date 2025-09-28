using System.Text.Json.Serialization;
using Peers.Modules.Listings.Domain.Logistics;

namespace Peers.Modules.Listings;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = false,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(FulfillmentPreferences))]
[JsonSerializable(typeof(LogisticsProfile))]
public partial class ListingsJsonSourceGenContext : JsonSerializerContext
{
}
