using System.Text.Json.Serialization;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Catalog;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = false,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(StringAttrConfig))]
[JsonSerializable(typeof(NumericAttrConfig<int>))]
[JsonSerializable(typeof(NumericAttrConfig<decimal>))]
[JsonSerializable(typeof(LookupAttrConfig))]
public partial class DomainJsonSourceGenContext : JsonSerializerContext
{
}
