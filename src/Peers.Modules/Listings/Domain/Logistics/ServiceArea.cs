using NetTopologySuite.Geometries;
using Peers.Core.Domain.Errors;
using Peers.Core.Geo;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Represents a circular geographic service area defined by a geographic center point and a radius in meters (m).
/// </summary>
/// <param name="Center">The geographic center point of the service area.</param>
/// <param name="Radius">The radius of the service area in meters (m).</param>
/// <remarks>
/// The service area is centered at a geographic coordinate (latitude and longitude) using the WGS 84
/// coordinate system (SRID 4326), with the radius specified in meters. This is used to determine whether
/// a given location falls within the defined area for purposes such as service coverage or delivery zones.
/// </remarks>
public sealed record ServiceArea(Point Center, double Radius)
{
    /// <summary>
    /// Determines whether the specified geographic point is within the defined radius of the center point using the
    /// Haversine distance formula.
    /// </summary>
    /// <param name="point">The geographic point to check for membership within the radius.</param>
    public bool Contains(Point point)
        => GeometryHelper.IsWithinDistance(Center, point, Radius);

    /// <summary>
    /// Validates that the latitude, longitude, and radius values are within their allowed ranges.
    /// </summary>
    internal void Validate()
    {
        if (Center is null)
        {
            throw new DomainException(E.Logistics.ServiceAreaCenterRequired);
        }
        // Generated server-side. Sanity check.
        if (Center.SRID != 4326)
        {
            throw new ArgumentException("Center point must use WGS 84 coordinate system (SRID 4326).", nameof(Center));
        }
        if (Center.Y is < (-90) or > 90)
        {
            throw new DomainException(E.Logistics.ServiceAreaCenterInvalidLatitude);
        }
        if (Center.X is < (-180) or > 180)
        {
            throw new DomainException(E.Logistics.ServiceAreaCenterInvalidLongitude);
        }
        if (Radius < 0)
        {
            throw new DomainException(E.Logistics.ServiceAreaRadiusMustBeNonNegative);
        }
    }
}
