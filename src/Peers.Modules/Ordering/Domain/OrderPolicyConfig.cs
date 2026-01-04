namespace Peers.Modules.Ordering.Domain;

/// <summary>Global platform policy for SLAs and complaint windows.</summary>
public sealed class OrderPolicyConfig
{
    /// <summary>Orders must be dispatched within this SLA once ready to ship.</summary>
    public TimeSpan DispatchSla { get; init; } = TimeSpan.FromHours(48);

    /// <summary>Seller must submit a quote within this window.</summary>
    public TimeSpan QuoteSubmissionDeadline { get; init; } = TimeSpan.FromHours(48);

    /// <summary>Buyer must accept a received quote within this window.</summary>
    public TimeSpan QuoteAcceptanceDeadline { get; init; } = TimeSpan.FromHours(24);

    /// <summary>Buyer can open a delivery complaint within this time from delivery.</summary>
    public TimeSpan BuyerComplaintWindow { get; init; } = TimeSpan.FromHours(72);

    /// <summary>Seller must respond to a complaint within this window.</summary>
    public TimeSpan SellerResponseWindow { get; init; } = TimeSpan.FromHours(48);

    /// <summary>Buyer must accept/reject a proposal within this window.</summary>
    public TimeSpan BuyerDecisionWindow { get; init; } = TimeSpan.FromHours(48);
}
