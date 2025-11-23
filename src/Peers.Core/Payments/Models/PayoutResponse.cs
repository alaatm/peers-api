namespace Peers.Core.Payments.Models;

public enum PayoutResponseEntryStatus
{
    /// <summary>
    /// The payout is pending.
    /// </summary>
    Pending,
    /// <summary>
    /// The payout is completed.
    /// </summary>
    Completed,
    /// <summary>
    /// The payout is failed.
    /// </summary>
    Failed,
}

public sealed class PayoutResponse
{
    public PayoutResponseEntry[] Entries { get; set; } = default!;
}

public sealed record PayoutResponseEntry
{
    public DateTime Timestamp { get; set; }
    public PayoutResponseEntryStatus Status { get; set; }
    public string? BatchId { get; set; }
    public string EntryId { get; set; } = default!;
    public string Iban { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal Total { get; set; }
    public string Message { get; set; } = default!;
}
