using System.Text.Json.Serialization;
using Peers.Modules.Listings.Domain;
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
[JsonSerializable(typeof(VariantAxesSnapshot))]
[JsonSerializable(typeof(VariantSelectionSnapshot))]
public partial class ListingsJsonSourceGenContext : JsonSerializerContext
{
}
