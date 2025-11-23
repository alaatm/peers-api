using System.Text.Json.Serialization;

namespace Peers.Core.Payments.Providers.ClickPay.Models;

public sealed class ClickPayErrorResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("trace")]
    public string Trace { get; set; } = default!;

    public override string ToString() => $"Code: {Code}\nMessage: {Message}\nTrace: {Trace}";
}
