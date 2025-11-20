using NetTopologySuite.Geometries;
using Peers.Core.Domain.Errors;
using E = Peers.Modules.Sellers.SellersErrors;

namespace Peers.Modules.Sellers.Domain;

/// <summary>
/// Represents a seller's shipping profile, including shipping rates, origin location, and applicable shipping policies.
/// </summary>
public sealed class ShippingProfile : Entity
{
    /// <summary>
    /// The unique identifier of the seller who owns this shipping profile.
    /// </summary>
    public int SellerId { get; set; }
    /// <summary>
    /// The name of the shipping profile.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// The seller who owns this shipping profile.
    /// </summary>
    public Seller Seller { get; set; } = default!;
    /// <summary>
    /// The geographical origin location from which shipments are sent.
    /// </summary>
    public Point OriginLocation { get; set; } = default!;
    /// <summary>
    /// The seller-managed shipping rate associated with this profile.
    /// </summary>
    public SellerManagedRate Rate { get; set; } = default!;
    /// <summary>
    /// The free shipping policy associated with this profile, if any.
    /// </summary>
    public FreeShippingPolicy? FreeShippingPolicy { get; set; }

    private ShippingProfile() { }

    internal ShippingProfile(
        string name,
        Seller seller,
        Point originLocation,
        SellerManagedRate rate,
        FreeShippingPolicy? freeShippingPolicy)
    {
        Name = name;
        Seller = seller;
        OriginLocation = originLocation;
        Rate = rate;
        FreeShippingPolicy = freeShippingPolicy;
    }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Shipping profile name cannot be null or whitespace.");
        }
        if (OriginLocation is null)
        {
            throw new DomainException(E.OriginLocationRequired);
        }
        if (Rate is null)
        {
            throw new DomainException(E.SellerRateRequired);
        }

        // Disallow FreeShippingPolicy with quote-based seller-managed rates.
        if (Rate.Kind is SellerManagedRateKind.Quote &&
            FreeShippingPolicy is not null)
        {
            throw new DomainException(E.FreeShippingPolicyNotAllowedForQuoteBasedShipping);
        }

        Rate.Validate();
        FreeShippingPolicy?.Validate();
    }
}
