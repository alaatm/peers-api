using System.Text.Json.Serialization;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Listings.Domain.Snapshots;

namespace Peers.Modules.Listings;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = false,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(FulfillmentPreferences))]
[JsonSerializable(typeof(LogisticsProfile))]
[JsonSerializable(typeof(ListingSnapshot))]
[JsonSerializable(typeof(VariantSelectionSnapshot))]
public partial class ListingsJsonSourceGenContext : JsonSerializerContext
{
}
