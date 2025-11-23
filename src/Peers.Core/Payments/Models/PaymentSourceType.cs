namespace Peers.Core.Payments.Models;

/// <summary>
/// The payment request source type.
/// </summary>
public enum PaymentSourceType
{
    /// <summary>
    /// Apple Pay source.
    /// </summary>
    ApplePay,
    /// <summary>
    /// Tokenized card source.
    /// </summary>
    TokenizedCard,
}
