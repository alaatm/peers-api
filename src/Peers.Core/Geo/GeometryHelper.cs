using System.Diagnostics.CodeAnalysis;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Point = NetTopologySuite.Geometries.Point;

namespace Peers.Core.Geo;

/// <summary>
/// Provides helper methods related to geometry and mapping.
/// </summary>
public static class GeometryHelper
{
    private const int Srid = 4326;
    private const double Rad = Math.PI / 180;
    private const double EarthRadius = 6371000.0;

    private static readonly GeometryFactory _gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);

    /// <summary>
    /// Creates a geometry point with the specified lat/lon coordinate.
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    public static Point CreatePoint(double lat, double lon)
        => _gf.CreatePoint(new Coordinate(lon, lat));

    /// <summary>
    /// Returns the straight line distance between two points.
    /// </summary>
    /// <param name="point1">The first point.</param>
    /// <param name="point2">The second point.</param>
    public static double DistanceBetween(
        [NotNull] Point point1,
        [NotNull] Point point2)
        => DistanceBetween(point1.Y, point1.X, point2.Y, point2.X);

    /// <summary>
    /// Returns the straight line distance between two points.
    /// </summary>
    /// <param name="lat1">Latitude of first point.</param>
    /// <param name="lon1">Longitude of first point.</param>
    /// <param name="lat2">Latitude of second point.</param>
    /// <param name="lon2">Longitude of second point.</param>
    public static double DistanceBetween(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        var lat1Rad = lat1 * Rad;
        var lat2Rad = lat2 * Rad;
        var dLat = (lat2 - lat1) * Rad;
        var dLon = (lon2 - lon1) * Rad;

        var haversine =
            (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) +
            (Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2));

        var center = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));

        return EarthRadius * center;
    }

    /// <summary>
    /// Returns whether point1 is within the specified distance in meters from point2.
    /// </summary>
    /// <param name="point1">The first point.</param>
    /// <param name="point2">The second point.</param>
    /// <param name="distance">The distance in meters.</param>
    /// <returns></returns>
    public static bool IsWithinDistance(
        [NotNull] Point point1,
        [NotNull] Point point2,
        double distance)
    {
        const double Error = 0.005;

        var d = DistanceBetween(point1.Y, point1.X, point2.Y, point2.X);
        return (d - (d * Error)) <= distance;
    }
}
