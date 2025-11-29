using System.Text.Json.Serialization;

namespace Peers.Core.Nafath.Models;

public enum NafathRequestStatus
{
    Completed,
    Expired,
    Rejected,
    Waiting
}

public sealed record NafathRetrieveRequestStatusResponse(
    [property: JsonPropertyName("status")] string Status)
{
    public NafathRequestStatus RequestStatus => Status switch
    {
        "COMPLETED" => NafathRequestStatus.Completed,
        "EXPIRED" => NafathRequestStatus.Expired,
        "REJECTED" => NafathRequestStatus.Rejected,
        "WAITING" => NafathRequestStatus.Waiting,
        _ => throw new InvalidOperationException($"Unknown status code: {Status}")
    };
}
