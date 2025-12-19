using Peers.Core.Common;

namespace Peers.Core.Payments;

public enum PaymentInfoIntent
{
    /// <summary>
    /// Indicates that the payment is intended for card tokenization.
    /// </summary>
    Tokenization,
    /// <summary>
    /// Indicates that the payment is intended for a hosted payment page transaction.
    /// </summary>
    HostedPaymentPage,
    /// <summary>
    /// Indicates that the payment is intended for a direct transaction via the payment provider's API.
    /// </summary>
    TransactionApi,
}

/// <summary>
/// Represents information about a payment.
/// </summary>
public sealed class PaymentInfo
{
    public const string OrderIdKey = "orderId";

    public PaymentInfoIntent Intent { get; private set; }
    /// <summary>
    /// The total monetary amount of the payment.
    /// </summary>
    public decimal Amount { get; private set; }
    /// <summary>
    /// The unique identifier for the order associated with the payment.
    /// </summary>
    public string OrderId { get; private set; }
    /// <summary>
    /// A description of the payment or its contents.
    /// </summary>
    public string Description { get; private set; }
    /// <summary>
    /// The customer's phone number associated with the payment.
    /// </summary>
    public string? CustomerPhone { get; private set; }
    /// <summary>
    /// The customer's email address associated with the payment.
    /// </summary>
    public string? CustomerEmail { get; private set; }
    /// <summary>
    /// Optional additional metadata related to the payment.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; private set; }

    private PaymentInfo(
        PaymentInfoIntent intent,
        decimal amount,
        string orderId,
        string description,
        string? customerPhone,
        string? customerEmail,
        Dictionary<string, string>? metadata)
    {
        if (amount.GetDecimalPlaces() > 2)
        {
            throw new ArgumentException("Amount must be in SAR and have a maximum of 2 decimal places.", nameof(amount));
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(amount, 0m, nameof(amount));
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId, nameof(orderId));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        Intent = intent;
        Amount = amount;
        OrderId = orderId;
        Description = description;
        CustomerPhone = customerPhone;
        CustomerEmail = customerEmail;
        Metadata = metadata is not null ? new Dictionary<string, string>(metadata) : null;
    }

    /// <summary>
    /// Creates a new instance of the PaymentInfo class for a card tokenization transaction with the specified details.
    /// </summary>
    /// <param name="customerId">The customer id.</param>
    /// <param name="customerPhone">The phone number of the customer making the payment.</param>
    /// <param name="customerEmail">The email address of the customer making the payment.</param>
    /// <returns>A PaymentInfo object containing the specified payment details.</returns>
    public static PaymentInfo ForTokenization(
        int customerId,
        string customerPhone,
        string customerEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerPhone, nameof(customerPhone));
        ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail, nameof(customerEmail));

        return new(
            intent: PaymentInfoIntent.Tokenization,
            amount: 1m,
            orderId: $"{customerId}",
            description: "Card Tokenization",
            customerPhone: customerPhone,
            customerEmail: customerEmail,
            metadata: null);
    }

    /// <summary>
    /// Creates a new instance of the PaymentInfo class for a HPP transaction with the specified details.
    /// </summary>
    /// <param name="amount">The amount to be paid for the transaction.</param>
    /// <param name="orderId">The unique identifier for the order associated with the payment.</param>
    /// <param name="description">A description of the payment or order.</param>
    /// <param name="customerPhone">The phone number of the customer making the payment.</param>
    /// <param name="customerEmail">The email address of the customer making the payment.</param>
    /// <param name="metadata">An optional collection of key-value pairs containing additional metadata to associate with the payment</param>
    /// <returns>A PaymentInfo object containing the specified payment details.</returns>
    public static PaymentInfo ForHpp(
        decimal amount,
        string orderId,
        string description,
        string customerPhone,
        string customerEmail,
        Dictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerPhone, nameof(customerPhone));
        ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail, nameof(customerEmail));

        return new(
            intent: PaymentInfoIntent.HostedPaymentPage,
            amount: amount,
            orderId: orderId,
            description: description,
            customerPhone: customerPhone,
            customerEmail: customerEmail,
            metadata: metadata);
    }

    /// <summary>
    /// Creates a new instance of the PaymentInfo class for a direct transaction api payment with the specified details.
    /// </summary>
    /// <param name="amount">The amount to be paid for the transaction.</param>
    /// <param name="orderId">The unique identifier for the order associated with the payment.</param>
    /// <param name="description">A description of the payment or order.</param>
    /// <param name="metadata">An optional collection of key-value pairs containing additional metadata to associate with the payment</param>
    /// <returns>A PaymentInfo object containing the specified payment details.</returns>
    public static PaymentInfo ForTransactionApi(
        decimal amount,
        string orderId,
        string description,
        Dictionary<string, string>? metadata = null) => new(
            intent: PaymentInfoIntent.TransactionApi,
            amount: amount,
            orderId: orderId,
            description: description,
            customerPhone: null,
            customerEmail: null,
            metadata: metadata);

    /// <summary>
    /// Sets the payment intent to indicate that a hosted payment page should be used for processing.
    /// </summary>
    internal void PromoteToHppIntent()
    {
        if (Intent is not PaymentInfoIntent.Tokenization)
        {
            throw new InvalidOperationException("Only payments with Tokenization intent can be promoted to HPP intent.");
        }

        Intent = PaymentInfoIntent.HostedPaymentPage;
    }
}
