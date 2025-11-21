using System.Text.Json.Serialization;

namespace Peers.Core.GoogleServices.Maps.Models.Geocoding;

/// <summary>
/// An object containing the response of a geocoding request.
/// </summary>
/// <param name="Status"> Contains the status of the request. </param>
/// <param name="ErrorMessage"> When status code is not "OK", this field contains more detailed information about the reasons behind the given status code. </param>
/// <param name="Results"> Contains an array of geocoded address information and geometry information. </param>
public sealed record GeocodeResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("error_message")] string? ErrorMessage,
    [property: JsonPropertyName("results")] GeocodeResult[] Results);

/// <summary>
/// An object containing the results of a geocoding request.
/// </summary>
/// <param name="AddressComponents"> An array containing the separate components applicable to this address. </param>
/// <param name="FormattedAddress"> A string containing the human-readable address of this location. </param>
public sealed record GeocodeResult(
    [property: JsonPropertyName("address_components")] AddressComponent[] AddressComponents,
    [property: JsonPropertyName("formatted_address")] string FormattedAddress);

/// <summary>
/// An object containing the separate components applicable to this address.
/// </summary>
/// <param name="LongName">The full text description or name of the address component as returned by the Geocoder.</param>
/// <param name="Types">An array indicating the type of the address component.</param>
public sealed record AddressComponent(
    [property: JsonPropertyName("long_name")] string LongName,
    [property: JsonPropertyName("types")] string[] Types);
