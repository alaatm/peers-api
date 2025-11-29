using System.Text.Json.Serialization;

namespace Peers.Core.Nafath.Models;

public sealed record NafathCallbackResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("transId")] Guid TransactionId,
    [property: JsonPropertyName("requestId")] Guid RequestId);
