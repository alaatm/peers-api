using Peers.Core.Domain.Errors;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Represents a policy that defines minimum and maximum order quantity constraints.
/// </summary>
/// <param name="Min">The minimum order quantity (must be ≥ 1).</param>
/// <param name="Max">The maximum order quantity (must be ≥ 1 and ≥ <paramref name="Min"/>).</param>
public record OrderQtyPolicy(int Min, int Max)
{
    /// <summary>
    /// Determines whether the specified quantity satisfies the defined minimum and maximum constraints.
    /// </summary>
    /// <param name="qty">The quantity to evaluate against the minimum and maximum limits.</param>
    public bool IsSatisfiedBy(int qty)
        => qty >= Min && qty <= Max;

    /// <summary>
    /// Ensures that the minimum and maximum order quantity values are valid.
    /// </summary>
    internal void Validate()
    {
        if (Min is < 1)
        {
            throw new DomainException(E.Logistics.MinOrderQtyMustBeAtLeastOne);
        }

        if (Max is < 1)
        {
            throw new DomainException(E.Logistics.MaxOrderQtyMustBeAtLeastOne);
        }

        if (Max < Min)
        {
            throw new DomainException(E.Logistics.MaxOrderQtyMustBeGreaterThanOrEqualToMin);
        }
    }
}
