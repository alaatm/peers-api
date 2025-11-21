using Peers.Core.GoogleServices.Maps.Models;
using Peers.Core.GoogleServices.Maps.Models.Geocoding;
using Peers.Core.GoogleServices.Maps.Models.SnapToRoads;

namespace Peers.Core.GoogleServices.Maps;

/// <summary>
/// Represents the Google Maps service.
/// </summary>
public interface IGoogleMapsService
{
    /// <summary>
    /// Reverse geocodes a point to an address.
    /// </summary>
    /// <param name="point">The point to reverse geocode.</param>
    /// <param name="lang">The language to use for the result.</param>
    /// <param name="ctk">The cancellation token.</param>
    Task<GeocodeResponse> ReverseGeocodeAsync(LatLng point, string lang, CancellationToken ctk = default);
    /// <summary>
    /// Takes up to 100 GPS points collected along a route, and returns a path that smoothly follows the geometry of the road.
    /// </summary>
    /// <param name="path">The GPS points.</param>
    /// <param name="ctk">The cancellation token.</param>
    Task<SnapToRoadsResponse> SnapToRoadsAsync(LatLng[] path, CancellationToken ctk = default);
    /// <summary>
    /// Retrieves the driving distance in meters between two geographic coordinates.
    /// </summary>
    /// <param name="src">The origin location as a latitude and longitude coordinate.</param>
    /// <param name="dst">The destination location as a latitude and longitude coordinate.</param>
    /// <param name="ctk">The cancellation token.</param>
    Task<int> GetDistanceAsync(LatLng src, LatLng dst, CancellationToken ctk = default);
}
