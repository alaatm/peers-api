using System.Text.Json.Serialization;

namespace Peers.Core.Nafath.Models;

public sealed record NafathRetrieveRequestStatusRequest(
    [property: JsonPropertyName("nationalId")] string NationalId,
    [property: JsonPropertyName("transId")] Guid TransactionId,
    [property: JsonPropertyName("random")] string Random);
