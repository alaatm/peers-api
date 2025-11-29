using System.Text.Json.Serialization;

namespace Peers.Core.Nafath.Models;

public sealed record NafathSendRequestResponse(
    [property: JsonPropertyName("transId")] Guid TransactionId,
    [property: JsonPropertyName("random")] string Random);
