using Peers.Core.Domain.Errors;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Represents a policy that defines optional minimum and maximum order quantity constraints.
/// </summary>
public readonly record struct OrderQtyPolicy
{
    public static OrderQtyPolicy Empty => new(null, null);

    public int? Min { get; }
    public int? Max { get; }

    private OrderQtyPolicy(int? min, int? max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Creates a new instance of the OrderQtyPolicy class with the specified minimum and maximum order quantities.
    /// </summary>
    /// <param name="min">The minimum allowed order quantity. Must be greater than or equal to 1, or null to indicate no minimum.</param>
    /// <param name="max">The maximum allowed order quantity. Must be greater than or equal to 1, or null to indicate no maximum.</param>
    internal static OrderQtyPolicy Create(int? min, int? max)
    {
        if (min is < 1)
        {
            throw new DomainException("Min order quantity must be ≥ 1.");
        }

        if (max is < 1)
        {
            throw new DomainException("Max order quantity must be ≥ 1.");
        }

        if (min.HasValue && max.HasValue && max.Value < min.Value)
        {
            throw new DomainException("Max order quantity must be ≥ min order quantity.");
        }

        return new OrderQtyPolicy(min, max);
    }

    /// <summary>
    /// Determines whether the specified quantity satisfies the defined minimum and maximum constraints.
    /// </summary>
    /// <param name="qty">The quantity to evaluate against the minimum and maximum limits.</param>
    /// <returns>true if the quantity is greater than or equal to the minimum value (if specified) and less than or equal to the
    /// maximum value (if specified); otherwise, false.</returns>
    internal bool IsSatisfiedBy(int qty)
        => (!Min.HasValue || qty >= Min.Value) && (!Max.HasValue || qty <= Max.Value);
}
