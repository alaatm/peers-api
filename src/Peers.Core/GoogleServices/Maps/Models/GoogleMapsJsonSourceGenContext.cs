using System.Text.Json.Serialization;
using Peers.Core.GoogleServices.Maps.Models.DistanceMatrix;
using Peers.Core.GoogleServices.Maps.Models.Geocoding;
using Peers.Core.GoogleServices.Maps.Models.SnapToRoads;

namespace Peers.Core.GoogleServices.Maps.Models;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = false,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(DistanceMatrixResponse))]
[JsonSerializable(typeof(GeocodeResponse))]
[JsonSerializable(typeof(SnapToRoadsResponse))]
public partial class GoogleMapsJsonSourceGenContext : JsonSerializerContext
{
}
