using System.Text.Json.Serialization;

namespace Peers.Core.GoogleServices.Maps.Models.DistanceMatrix;

/// <summary>
/// Represents the response returned from a distance matrix service, containing travel distances and durations between
/// pairs of origin and destination addresses.
/// </summary>
/// <param name="DestinationAddresses">An array of formatted addresses corresponding to the destinations specified in the request. The order matches the
/// destinations in the response matrix.</param>
/// <param name="OriginAddresses">An array of formatted addresses corresponding to the origins specified in the request. The order matches the origins
/// in the response matrix.</param>
/// <param name="Rows">An array of rows, each containing distance and duration information for a single origin to all specified
/// destinations. The number of rows matches the number of origins.</param>
/// <param name="Status">The overall status of the response, indicating whether the request was successful or if an error occurred. Typical
/// values include "OK" or error codes.</param>
/// <param name="ErrorMessage">An optional error message providing additional details if the request failed or was only partially successful. This
/// value is null if the request was successful.</param>
public sealed record DistanceMatrixResponse(
    [property: JsonPropertyName("destination_addresses")] string[] DestinationAddresses,
    [property: JsonPropertyName("origin_addresses")] string[] OriginAddresses,
    [property: JsonPropertyName("rows")] DistanceMatrixRow[] Rows,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("error_message")] string? ErrorMessage
);

/// <summary>
/// Represents a single row in a distance matrix, containing the results for each destination from a specific origin.
/// </summary>
/// <param name="Elements">An array of <see cref="DistanceMatrixElement"/> objects representing the results for each destination in this row.
/// Cannot be null.</param>
public sealed record DistanceMatrixRow(
    [property: JsonPropertyName("elements")] DistanceMatrixElement[] Elements
);

/// <summary>
/// Represents a single element in a distance matrix, containing the calculated distance between two locations.
/// </summary>
/// <param name="Distance">The distance information for this matrix element. Cannot be null.</param>
public sealed record DistanceMatrixElement(
    [property: JsonPropertyName("distance")] Distance Distance
);

/// <summary>
/// Represents a distance measurement with both a human-readable string and a numeric value.
/// </summary>
/// <param name="Text">The distance as a human-readable string, such as "5 km" or "3.1 miles". Cannot be null.</param>
/// <param name="Value">The distance value in meters.</param>
public sealed record Distance(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("value")] int Value
);
