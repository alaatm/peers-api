using Peers.Core.Nafath.Models;

namespace Peers.Modules.Sellers.Domain;

/// <summary>
/// Represents the Nafath identity information of a seller.
/// </summary>
public sealed class NafathInfo : Entity
{
    /// <summary>
    /// The national ID / Iqama number of the seller.
    /// </summary>
    public string NationalId { get; init; } = default!;
    /// <summary>
    /// The first name of the seller in Arabic.
    /// </summary>
    public string? FirstNameAr { get; init; }
    /// <summary>
    /// The last name of the seller in Arabic.
    /// </summary>
    public string? LastNameAr { get; init; }
    /// <summary>
    /// The first name of the seller in English.
    /// </summary>
    public string? FirstNameEn { get; init; }
    /// <summary>
    /// The last name of the seller in English.
    /// </summary>
    public string? LastNameEn { get; init; }
    /// <summary>
    /// The gender of the seller.
    /// </summary>
    public string? Gender { get; init; }

    /// <summary>
    /// Creates a new instance of <see cref="NafathInfo"/> using the data from the specified <see cref="NafathIdentity"/>.
    /// </summary>
    /// <param name="identity">The source identity.</param>
    internal static NafathInfo FromNafathIdentity([NotNull] NafathIdentity identity) => new()
    {
        NationalId = identity.NationalId,
        FirstNameAr = identity.FirstNameAr,
        LastNameAr = identity.LastNameAr,
        FirstNameEn = identity.FirstNameEn,
        LastNameEn = identity.LastNameEn,
        Gender = identity.Gender,
    };
}
