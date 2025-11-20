using System.Text.Json.Serialization;
using Peers.Modules.Sellers.Domain;

namespace Peers.Modules.Sellers;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = false,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(SellerManagedRate))]
[JsonSerializable(typeof(FreeShippingPolicy))]
public partial class SellersJsonSourceGenContext : JsonSerializerContext
{
}
