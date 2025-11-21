using System.Text.Json.Serialization;

namespace Peers.Core.GoogleServices.Maps.Models.SnapToRoads;

/// <summary>
/// Represents the response from a Snap to Roads API call, containing the snapped points and any associated warning
/// message.
/// </summary>
/// <param name="SnappedPoints">A list of points snapped to the road, in the order of the input path.</param>
/// <param name="WarningMessage">A string containing a user-visible warning.</param>
public sealed record SnapToRoadsResponse(
    [property: JsonPropertyName("snappedPoints")] SnappedPoint[] SnappedPoints,
    [property: JsonPropertyName("warningMessage")] string? WarningMessage);

/// <summary>
/// Represents a geographic point that has been adjusted, or 'snapped', to a specific location, such as a road or path.
/// </summary>
/// <param name="Location">The location on the road.</param>
public sealed record SnappedPoint(
    [property: JsonPropertyName("location")] LatitudeLongitudeLiteral Location
);

/// <summary>
/// An object describing a specific location with Latitude and Longitude in decimal degrees.
/// </summary>
/// <param name="Latitude">Latitude in decimal degrees.</param>
/// <param name="Longitude">Longitude in decimal degrees.</param>
public record LatitudeLongitudeLiteral(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude);
