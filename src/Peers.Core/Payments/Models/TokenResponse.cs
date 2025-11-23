namespace Peers.Core.Payments.Models;

/// <summary>
/// Represents a tokenized card response from a payment gateway.
/// </summary>
public sealed class TokenResponse
{
    /// <summary>
    /// The card brand (e.g., Visa, MasterCard).
    /// </summary>
    public PaymentCardBrand CardBrand { get; set; }
    /// <summary>
    /// The card type (e.g., credit, debit).
    /// </summary>
    public PaymentCardFunding CardType { get; set; }
    /// <summary>
    /// The masked card number (e.g., **** **** **** 1234).
    /// </summary>
    public string MaskedCardNumber { get; set; } = default!;
    /// <summary>
    /// The expiry month of the card (1-12).
    /// </summary>
    public int ExpiryMonth { get; set; }
    /// <summary>
    /// The expiry year of the card (e.g., 2025).
    /// </summary>
    public int ExpiryYear { get; set; }
    /// <summary>
    /// The expiry date of the card as a DateTime object.
    /// </summary>
    public DateOnly Expiry => PaymentCardUtils.GetExpiryDate(ExpiryYear, ExpiryMonth);
}
