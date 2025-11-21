using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Peers.Core.GoogleServices.Configuration;
using Peers.Core.GoogleServices.Maps.Models;
using Peers.Core.GoogleServices.Maps.Models.Geocoding;
using Peers.Core.GoogleServices.Maps.Models.SnapToRoads;

namespace Peers.Core.GoogleServices.Maps;

/// <summary>
/// Represents the Google Maps service.
/// </summary>
public sealed class GoogleMapsService : IGoogleMapsService
{
    private static readonly CompositeFormat _reverseGeocodeCacheKey = CompositeFormat.Parse("REVGEOCODE:{0},{1}:{2}");
    private static readonly CompositeFormat _distanceCacheKey = CompositeFormat.Parse("DIST:{0},{1}:{2},{3}");
    private static readonly TimeSpan _defaultCacheDuration = TimeSpan.FromHours(6);

    private readonly HttpClient _httpClient;
    private readonly GoogleConfig _config;
    private readonly IMemoryCache _cache;

    public GoogleMapsService(
        HttpClient httpClient,
        GoogleConfig config,
        IMemoryCache cache)
    {
        _config = config;
        _httpClient = httpClient;
        _cache = cache;
    }

    /// <summary>
    /// Reverse geocodes a point to an address.
    /// </summary>
    /// <param name="point">The point to reverse geocode.</param>
    /// <param name="lang">The language to use for the result.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns></returns>
    public async Task<GeocodeResponse> ReverseGeocodeAsync([NotNull] LatLng point, string lang, CancellationToken ctk = default)
    {
        var cacheKey = string.Format(CultureInfo.InvariantCulture, _reverseGeocodeCacheKey, point.Latitude, point.Longitude, lang);
        if (_cache.TryGetValue<GeocodeResponse>(cacheKey, out var cached))
        {
            return cached!;
        }

        var url = new Uri(
            $"https://maps.googleapis.com/maps/api/geocode/json?latlng={point.Latitude},{point.Longitude}&result_type=street_address&language={lang}&key={_config.ApiKey}",
            UriKind.Absolute);

        using var response = await _httpClient.GetAsync(url, ctk);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync(GoogleMapsJsonSourceGenContext.Default.GeocodeResponse, ctk))!;

        _cache.Set(cacheKey, result, _defaultCacheDuration);
        return result;
    }

    /// <summary>
    /// Takes up to 100 GPS points collected along a route, and returns a path that smoothly follows the geometry of the road.
    /// </summary>
    /// <param name="path">The GPS points.</param>
    /// <param name="ctk">The cancellation token.</param>
    /// <returns></returns>
    public async Task<SnapToRoadsResponse> SnapToRoadsAsync([NotNull] LatLng[] path, CancellationToken ctk = default)
    {
        var url = new Uri(
            $"https://roads.googleapis.com/v1/snapToRoads?interpolate=true&path={CombineLatLng(path)}&key={_config.ApiKey}",
            UriKind.Absolute);

        using var response = await _httpClient.GetAsync(url, ctk);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync(GoogleMapsJsonSourceGenContext.Default.SnapToRoadsResponse, ctk))!;
    }

    /// <summary>
    /// Retrieves the driving distance in meters between two geographic coordinates.
    /// </summary>
    /// <param name="src">The origin location as a latitude and longitude coordinate.</param>
    /// <param name="dst">The destination location as a latitude and longitude coordinate.</param>
    /// <param name="ctk">The cancellation token.</param>
    public async Task<int> GetDistanceAsync([NotNull] LatLng src, [NotNull] LatLng dst, CancellationToken ctk = default)
    {
        var cacheKey = string.Format(CultureInfo.InvariantCulture, _distanceCacheKey, src.Latitude, src.Longitude, dst.Latitude, dst.Longitude);
        if (_cache.TryGetValue<int>(cacheKey, out var cached))
        {
            return cached;
        }

        var url = new Uri(
            $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={src.Latitude},{src.Longitude}&destinations={dst.Latitude},{dst.Longitude}&key={_config.ApiKey}",
            UriKind.Absolute);

        using var response = await _httpClient.GetAsync(url, ctk);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync(GoogleMapsJsonSourceGenContext.Default.DistanceMatrixResponse, ctk))!;
        var distance = result.Rows[0].Elements[0].Distance.Value;

        _cache.Set(cacheKey, distance, _defaultCacheDuration);
        return distance;
    }

    private static string CombineLatLng(LatLng[] points)
    {
        const string Format = "F6";

        Debug.Assert(points.Length is > 0 and <= 100);

        // First pass: calculate required length, intialize with pipe separator count.
        var totalLength = points.Length - 1;
        for (var i = 0; i < points.Length; i++)
        {
            totalLength += GetLength(points[i].Latitude) + 1 + GetLength(points[i].Longitude);
        }

        // Second pass: create string with the exact size.
        var culture = CultureInfo.InvariantCulture;
        return string.Create(totalLength, points, (span, pts) =>
        {
            var pos = 0;
            var iLast = pts.Length - 1;
            for (var i = 0; i < iLast; i++)
            {
                WritePoint(pts[i], span, ref pos);
                span[pos++] = '|';
            }

            // Process the last element.
            WritePoint(pts[iLast], span, ref pos);
        });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WritePoint(LatLng point, Span<char> span, ref int pos)
        {
            _ = point.Latitude.TryFormat(span[pos..], out var latLen, Format, culture);
            pos += latLen;
            span[pos++] = ',';
            _ = point.Longitude.TryFormat(span[pos..], out var lonLen, Format, culture);
            pos += lonLen;
        }

        static int GetLength(double value) => value < 0
            // Negative sign, integer part, decimal point, 6 decimal places.
            ? 1 + GetIntLength(-value) + 1 + 6
            // Integer part, decimal point, 6 decimal places.
            : GetIntLength(value) + 1 + 6;

        static int GetIntLength(double value) => value switch
        {
            >= 100 => 3,
            >= 10 => 2,
            _ => 1
        };
    }
}
