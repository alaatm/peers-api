using System.Text.Json.Serialization;
using Peers.Core.Payments.Models;

namespace Peers.Core.Payments.Providers.Moyasar.Models.Payouts;

public class MoyasarPayoutResponse
{
    [JsonPropertyName("payouts")]
    public MoyasarPayoutResponseEntry[] Entries { get; set; } = default!;

    public PayoutResponse ToGeneric()
    {
        var entries = new PayoutResponseEntry[Entries.Length];

        for (var i = 0; i < entries.Length; i++)
        {
            entries[i] = Entries[i].ToGeneric();
        }

        return new PayoutResponse
        {
            Entries = entries,
        };
    }
}

public sealed class MoyasarPayoutResponseEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = default!;

    [JsonPropertyName("sequence_number")]
    public string SequenceNumber { get; set; } = default!;

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = default!;

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("destination")]
    public MoyasarPayoutDestination Destination { get; set; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("failure_reason")]
    public string? FailureReason { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; } = default!;

    public PayoutResponseEntry ToGeneric() => new()
    {
        Timestamp = UpdatedAt,
        Status = Status switch
        {
            "paid" => PayoutResponseEntryStatus.Completed,
            "failed" or "canceled" or "returned" => PayoutResponseEntryStatus.Failed,
            _ => PayoutResponseEntryStatus.Pending,
        },
        EntryId = Id,
        Iban = Destination.Iban,
        Currency = Currency,
        Amount = Amount / 100m,
        Total = Amount / 100m,
        Message = Message + FailureReason switch
        {
            null => string.Empty,
            _ => $": {FailureReason}",
        },
    };
}
