namespace Peers.Core.Payments.Models;

/// <summary>
/// Represents the response returned when initializing a hosted payment page, providing integration details for
/// rendering or redirecting the customer to complete payment.
/// </summary>
/// <remarks>Depending on the payment provider, either the <see cref="Script"/> property or the <see
/// cref="RedirectUrl"/> property will be populated.</remarks>
public sealed class HostedPagePaymentInitResponse
{
    /// <summary>
    /// A script to be embedded in the merchant's webpage to render the payment UI.
    /// This is used exclusively by Moyasar hosted pages.
    /// </summary>
    public string? Script { get; init; }
    /// <summary>
    /// A redirect URL to which the customer should be redirected to complete the payment.
    /// This is used exclusively by ClickPay hosted pages.
    /// </summary>
    public Uri? RedirectUrl { get; init; }
}
