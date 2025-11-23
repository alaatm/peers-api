using System.Diagnostics;
using Peers.Core.Payments;

namespace Peers.Modules.Customers.Domain;

/// <summary>
/// Represents a payment method.
/// </summary>
public abstract class PaymentMethod : Entity
{
    /// <summary>
    /// The type of the payment method.
    /// </summary>
    public PaymentType Type { get; set; }
    /// <summary>
    /// The date/time at which this payment method was added.
    /// </summary>
    public DateTime AddedOn { get; set; }
    /// <summary>
    /// Indicates whether this payment method is the default.
    /// </summary>
    public bool IsDefault { get; set; }
    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; private set; }
    /// <summary>
    /// The date/time at which this payment method was deleted.
    /// </summary>
    public DateTime? DeletedOn { get; set; }

    internal void Delete(DateTime date)
    {
        Debug.Assert(Type is PaymentType.Card);
        DeletedOn = date;
        IsDeleted = true;
    }
}

/// <summary>
/// Represents ApplePay payment method.
/// </summary>
public class ApplePay : PaymentMethod
{
    /// <summary>
    /// Indicates whether this payment method is active.
    /// </summary>
    public bool IsActive { get; set; }

    internal static ApplePay Create(bool isActive, DateTime date) => new()
    {
        Type = PaymentType.ApplePay,
        AddedOn = date,
        IsActive = isActive,
    };
}

/// <summary>
/// Represents Cash payment method.
/// </summary>
public class Cash : PaymentMethod
{
    internal static Cash Create(DateTime date) => new()
    {
        Type = PaymentType.Cash,
        AddedOn = date,
    };
}

/// <summary>
/// Represents credit/debit card payment method.
/// </summary>
public class PaymentCard : PaymentMethod
{
    /// <summary>
    /// The id of the payment that was used to tokenize this card.
    /// </summary>
    public string PaymentId { get; set; } = default!;
    /// <summary>
    /// The credit card type.
    /// </summary>
    public PaymentCardBrand? Brand { get; set; }
    /// <summary>
    /// The credit card funding type.
    /// Either credit or debit.
    /// </summary>
    public PaymentCardFunding? Funding { get; set; }
    /// <summary>
    /// The first 6 digits of the card number.
    /// </summary>
    public string First6Digits { get; set; } = default!;
    /// <summary>
    /// The last 4 digits of the card number.
    /// </summary>
    public string Last4Digits { get; set; } = default!;
    /// <summary>
    /// The credit card expiry date.
    /// </summary>
    public DateOnly? Expiry { get; set; }
    /// <summary>
    /// The payment gateway token.
    /// </summary>
    public string Token { get; set; } = default!;
    /// <summary>
    /// Indicates whether this card is activated and verified on our side.
    /// </summary>
    /// <remarks>
    /// This field defaults to false when the card is created. Once PaymentGateway calls return url with successe,
    /// we should void the payment, update any missing card metadata then set the card as verified.
    /// </remarks>
    public bool IsVerified { get; set; }
    /// <summary>
    /// Indicates whether this card is expired.
    /// </summary>
    public bool IsExpired(DateOnly date) => PaymentCardUtils.IsExpired(Expiry, date);

    internal static PaymentCard Create(
        string paymentId,
        PaymentCardBrand? brand,
        PaymentCardFunding? funding,
        string maskedNumber,
        DateOnly? expiry,
        string token,
        DateTime date)
    {
        var (first6Digits, last4Digits) = ExtractMaskedCardNumberDigits(maskedNumber);

        return new()
        {
            Type = PaymentType.Card,
            PaymentId = paymentId,
            Brand = brand,
            Funding = funding,
            First6Digits = first6Digits,
            Last4Digits = last4Digits,
            Expiry = expiry,
            Token = token,
            AddedOn = date,
            IsVerified = false,
        };
    }

    private static (string first6, string last4) ExtractMaskedCardNumberDigits(string maskedNumber)
    {
        // [in] Moyasar      => 4111-11XX-XXXX-1111
        // [in] ClickPay     => 4111 11## #### 1111
        // extract first 6   => 411111
        // extract last 4    => 1111

        try
        {
            maskedNumber ??= string.Empty;
            maskedNumber = maskedNumber
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal);

            var first6 = maskedNumber[..6];
            var last4 = maskedNumber[^4..];

            return (first6, last4);
        }
        catch (ArgumentOutOfRangeException)
        {
            return (maskedNumber, maskedNumber);
        }
    }
}
