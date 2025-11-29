using System.Diagnostics;
using NetTopologySuite.Geometries;
using Peers.Core.Domain.Errors;
using Peers.Core.Nafath.Models;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Users.Domain;
using E = Peers.Modules.Sellers.SellersErrors;

namespace Peers.Modules.Sellers.Domain;

public sealed class Seller : Customer, IAggregateRoot
{
    /// <summary>
    /// The Nafath identity information of the seller.
    /// </summary>
    public NafathInfo Nafath { get; set; } = default!;
    /// <summary>
    /// The list of shipping profiles associated with the seller.
    /// </summary>
    public List<ShippingProfile> ShippingProfiles { get; set; } = default!;
    /// <summary>
    /// The list of listings associated with the seller.
    /// </summary>
    public List<Listing> Listings { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of <see cref="Seller"/>.
    /// </summary>
    /// <param name="user">The database user.</param>
    /// <param name="nafathIdentity">The verified nafath identity.</param>
    /// <returns></returns>
    public static Seller Create(
        [NotNull] AppUser user,
        [NotNull] NafathIdentity nafathIdentity)
    {
        Debug.Assert(user.UserName is not null);

        var seller = new Seller
        {
            User = user,
            Username = user.UserName,
            Nafath = NafathInfo.FromNafathIdentity(nafathIdentity),
            ShippingProfiles = [],
            Listings = [],
        };

        return seller;
    }

    /// <summary>
    /// Creates a new shipping profile with the specified name, origin location, rate, and free shipping policy.
    /// </summary>
    /// <param name="name">The unique name of the shipping profile to create. Cannot be null or empty. Leading and trailing whitespace is
    /// ignored.</param>
    /// <param name="originLocation">The origin location for shipments associated with this profile. Cannot be null.</param>
    /// <param name="rate">The seller-managed rate to apply to shipments using this profile. Cannot be null.</param>
    /// <param name="freeShippingPolicy">The free shipping policy to associate with this profile. May be null if no free shipping policy is required.</param>
    /// <exception cref="DomainException">Thrown if a shipping profile with the specified name already exists.</exception>
    public void CreateShippingProfile(
        [NotNull] string name,
        [NotNull] Point originLocation,
        [NotNull] SellerManagedRate rate,
        FreeShippingPolicy freeShippingPolicy)
    {
        name = name.Trim();
        if (ShippingProfiles.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is not null)
        {
            throw new DomainException(E.DuplicateShippingProfileName(name));
        }

        var profile = new ShippingProfile(
            name,
            this,
            originLocation,
            rate,
            freeShippingPolicy);

        profile.Validate();
        ShippingProfiles.Add(profile);
    }
}
