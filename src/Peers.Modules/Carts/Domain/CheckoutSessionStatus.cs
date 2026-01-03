namespace Peers.Modules.Carts.Domain;

/// <summary>
/// Specifies the status of a checkout session, indicating its current state in the checkout process.
/// Terminal states are Completed, Failed, Expired, and Invalidated.
/// Non-terminal states are Active, IntentIssued or Paying. Only one non-terminal session can exist per cart at any time.
/// </summary>
public enum CheckoutSessionStatus
{
    /// <summary>
    /// Indicates that the checkout session is currently active and in progress.
    /// </summary>
    Active,
    /// <summary>
    /// Indicates that a payment intent has been issued for the session as either hosted payment page (HPP) or API.
    /// </summary>
    IntentIssued,
    /// <summary>
    /// Indicates that the checkout session is in the process of payment (Payment gateway interaction).
    /// </summary>
    Paying,
    /// <summary>
    /// Indicates that the checkout session has been successfully completed.
    /// </summary>
    Completed,
    /// <summary>
    /// Indicates that the checkout session has failed.
    /// </summary>
    Failed,
    /// <summary>
    /// Indicates that the checkout session has expired and is no longer valid.
    /// </summary>
    Expired,
    /// <summary>
    /// Indicates that the checkout session has been invalidated.
    /// </summary>
    Invalidated,
}
