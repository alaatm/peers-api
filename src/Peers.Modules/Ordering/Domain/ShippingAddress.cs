using NetTopologySuite.Geometries;
using Peers.Core.Geo;

namespace Peers.Modules.Ordering.Domain;

/// <summary>
/// Represents a complete, immutable postal address for shipping purposes.
/// </summary>
/// <param name="FullName">The full name of the recipient for the shipping address.</param>
/// <param name="PhoneNumber">The contact phone number for the recipient.</param>
/// <param name="BuildingNumber">The building or house number component of the address.</param>
/// <param name="ApartmentNumber">The apartment, suite, or unit number within the building, if applicable; otherwise, null.</param>
/// <param name="Street">The street name for the address.</param>
/// <param name="District">The district, neighborhood, or locality within the city.</param>
/// <param name="City">The city or municipality for the address.</param>
/// <param name="Governorate">The governorate, state, or administrative region for the address.</param>
/// <param name="Country">The country in which the address is located.</param>
/// <param name="FormattedAddress">A human-readable, formatted representation of the full address.</param>
/// <param name="Location">The geographic coordinates representing the location of the address.</param>
public sealed record ShippingAddress(
    string FullName,
    string PhoneNumber,
    string BuildingNumber,
    string? ApartmentNumber,
    string Street,
    string District,
    string City,
    string Governorate,
    string Country,
    string FormattedAddress,
    Point Location
)
{
    public static ShippingAddress FromAddress(string fullName, string phoneNumber, [NotNull] Address address)
        => new(
            FullName: fullName,
            PhoneNumber: phoneNumber,
            BuildingNumber: address.BuildingNumber,
            ApartmentNumber: address.ApartmentNumber,
            Street: address.Street,
            District: address.District,
            City: address.City,
            Governorate: address.Governorate,
            Country: address.Country,
            FormattedAddress: address.FormattedAddress,
            Location: GeometryHelper.CreatePoint(address.Location.Y, address.Location.X)
        );
}
