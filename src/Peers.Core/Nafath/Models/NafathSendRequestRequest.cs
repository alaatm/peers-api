using System.Text.Json.Serialization;

namespace Peers.Core.Nafath.Models;

public sealed record NafathSendRequestRequest(
    [property: JsonPropertyName("nationalId")] string NationalId,
    [property: JsonPropertyName("service")] string Service = "DigitalServiceEnrollmentWithoutBio");
