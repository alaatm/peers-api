using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Peers.Core.Payments.Models;

namespace Peers.Core.Payments.Providers.Moyasar.Models.Payouts;

public sealed class MoyasarPayoutRequest
{
    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = default!;
    [JsonPropertyName("payouts")]
    public MoyasarPayoutRequestEntry[] Entries { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of <see cref="MoyasarPayoutRequest"/>.
    /// </summary>
    /// <param name="sourceId">The source bank ID.</param>
    /// <param name="request">The payout request containing entries to be processed.</param>
    /// <returns></returns>
    public static MoyasarPayoutRequest Create(string sourceId, [NotNull] PayoutRequest request)
    {
        var entries = new MoyasarPayoutRequestEntry[request.Entries.Count];

        for (var i = 0; i < entries.Length; i++)
        {
            entries[i] = MoyasarPayoutRequestEntry.FromGeneric(request.Entries[i]);
        }

        return new MoyasarPayoutRequest
        {
            SourceId = sourceId,
            Entries = entries,
        };
    }
}

public sealed class MoyasarPayoutRequestEntry
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    [JsonPropertyName("purpose")]
    public string Purpose => "payment_to_merchant";
    [JsonPropertyName("destination")]
    public MoyasarPayoutDestination Destination { get; set; } = default!;
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    public static MoyasarPayoutRequestEntry FromGeneric([NotNull] PayoutRequestEntry entry) => new()
    {
        Amount = (int)(entry.Amount * 100),
        Destination = new MoyasarPayoutDestination
        {
            Type = "bank",
            Iban = entry.Bank.Iban,
            BeneficiaryName = entry.Beneficiary.Name,
            PhoneNumber = entry.Beneficiary.PhoneNumber,
            Country = entry.Beneficiary.Country,
            City = entry.Beneficiary.City,
        },
        Comment = entry.Beneficiary.EmailAddress,
        Metadata = entry.Metadata,
    };
}
