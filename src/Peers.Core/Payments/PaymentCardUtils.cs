using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Payments;

/// <summary>
/// Provides utility methods for handling payment card information.
/// </summary>
public static class PaymentCardUtils
{
    /// <summary>
    /// Calculates the expiry date of a card based on the given year and month.
    /// </summary>
    /// <param name="expiryYear">The expiry year of the card.</param>
    /// <param name="expiryMonth">The expiry month of the card.</param>
    /// <returns></returns>
    public static DateOnly GetExpiryDate(int expiryYear, int expiryMonth)
        => new DateOnly(expiryYear, expiryMonth, 1).AddMonths(1).AddDays(-1);

    /// <summary>
    /// Checks if the card is expired based on the given date.
    /// </summary>
    /// <param name="expiryDate">The expiry date of the card.</param>
    /// <param name="date">The date to check against.</param>
    /// <returns></returns>
    public static bool IsExpired(DateOnly? expiryDate, DateOnly date)
        => !expiryDate.HasValue || expiryDate.Value < date;

    /// <summary>
    /// Resolves the card brand based on the given string value.
    /// </summary>
    /// <param name="value">The string value representing the card brand.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static PaymentCardBrand ResolveCardBrand([NotNull] string value) => value.ToUpperInvariant() switch
    {
        "AMEX" or "AMERICANEXPRESS" or "AMERICAN_EXPRESS" => PaymentCardBrand.Amex,
        "MASTERCARD" or "MASTER_CARD" or "MASTER" => PaymentCardBrand.MasterCard,
        "VISA" => PaymentCardBrand.Visa,
        "MADA" => PaymentCardBrand.Mada,
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    /// <summary>
    /// Resolves the card funding type based on the given string value.
    /// </summary>
    /// <param name="value">The string value representing the card funding type.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static PaymentCardFunding ResolveCardFunding([NotNull] string value) => value.ToUpperInvariant() switch
    {
        "CREDIT" => PaymentCardFunding.Credit,
        "DEBIT" => PaymentCardFunding.Debit,
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };
}
