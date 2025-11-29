using Peers.Core.Nafath.Models;
using System.Text.Json.Serialization;

namespace Peers.Core.Nafath;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = false,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(NafathCallbackResponse))]
[JsonSerializable(typeof(NafathErrorResponse))]
[JsonSerializable(typeof(NafathRetrieveRequestStatusRequest))]
[JsonSerializable(typeof(NafathRetrieveRequestStatusResponse))]
[JsonSerializable(typeof(NafathSendRequestRequest))]
[JsonSerializable(typeof(NafathSendRequestResponse))]
public partial class NafathJsonSourceGenContext : JsonSerializerContext
{
}
