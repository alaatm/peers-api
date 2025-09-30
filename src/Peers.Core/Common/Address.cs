using NetTopologySuite.Geometries;

namespace Peers.Core.Common;

/// <summary>
/// Represents a complete, immutable postal address.
/// </summary>
/// <param name="BuildingNumber">The building or house number component of the address.</param>
/// <param name="ApartmentNumber">The apartment, suite, or unit number within the building, if applicable; otherwise, null.</param>
/// <param name="Street">The street name for the address.</param>
/// <param name="District">The district, neighborhood, or locality within the city.</param>
/// <param name="City">The city or municipality for the address.</param>
/// <param name="Governorate">The governorate, state, or administrative region for the address.</param>
/// <param name="Country">The country in which the address is located.</param>
/// <param name="FormattedAddress">A human-readable, formatted representation of the full address.</param>
/// <param name="Location">The geographic coordinates representing the location of the address.</param>
public sealed record Address(
    string BuildingNumber,
    string? ApartmentNumber,
    string Street,
    string District,
    string City,
    string Governorate,
    string Country,
    string FormattedAddress,
    Point Location
);
