namespace Peers.Modules.Carts.Services;

/// <summary>
/// Specifies the possible outcomes of a shipping cost calculation attempt.
/// </summary>
public enum ShippingCalculationOutcome
{
    /// <summary>
    /// Indicates that the operation completed successfully.
    /// </summary>
    Success,
    /// <summary>
    /// Gets or sets a value indicating whether a quote is required for the current operation.
    /// </summary>
    QuoteRequired
}

/// <summary>
/// Represents the result of a shipping cost calculation, including the outcome and the total calculated amount.
/// </summary>
/// <param name="Outcome">The outcome of the shipping calculation, indicating whether a total was determined or if a quote is required.</param>
/// <param name="Total">The total shipping cost calculated. The value is zero if a quote is required.</param>
public sealed record ShippingCalculatorResult(
    ShippingCalculationOutcome Outcome,
    decimal Total
)
{
    /// <summary>
    /// Creates a result indicating that a shipping quote is required before proceeding.
    /// </summary>
    public static ShippingCalculatorResult QuoteRequired()
        => new(ShippingCalculationOutcome.QuoteRequired, 0m);
}
