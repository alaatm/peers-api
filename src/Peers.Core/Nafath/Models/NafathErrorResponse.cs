using System.Text.Json.Serialization;

namespace Peers.Core.Nafath.Models;

public sealed record NafathErrorResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("timestamp")] string? Timestamp,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("reference")] int Reference)
{
    public bool IsInvalidRequestData => Code == "422-031-046";
}
