using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace Peers.Core.GoogleServices.Maps.Models;

/// <summary>
/// Represents a latitude/longitude pair.
/// </summary>
/// <param name="Latitude">The latitude.</param>
/// <param name="Longitude">The longitude.</param>
public record LatLng(
    [property: JsonPropertyName("lat")] double Latitude,
    [property: JsonPropertyName("lng")] double Longitude)
{
    /// <summary>
    /// Creates a new LatLng instance from the specified Point.
    /// </summary>
    /// <param name="point">The Point whose Y and X coordinates will be used as latitude and longitude, respectively.</param>
    public static LatLng FromPoint([NotNull] Point point) => new(point.Y, point.X);
}
