using System.Security.Cryptography;

namespace Peers.Modules.Users.Domain;

/// <summary>
/// Represents a JWT refresh token to be used to obtain new JWT tokens and provide the ability
/// to revoke them for security purposes.
/// </summary>
public sealed class RefreshToken
{
    /// <summary>
    /// The refresh token.
    /// </summary>
    public string Token { get; set; } = default!;
    /// <summary>
    /// The date and time when this token was created.
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// If revoked, contains the date and time the revoke took place.
    /// </summary>
    public DateTime? Revoked { get; set; }
    /// <summary>
    /// Indicates whether the refresh token is active.
    /// </summary>
    public bool IsActive => Revoked is null;

    /// <summary>
    /// Initializes a new instance of <see cref="RefreshToken"/>.
    /// </summary>
    /// <param name="date">The date when the token was created.</param>
    /// <returns></returns>
    internal static RefreshToken Create(DateTime date) => new()
    {
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
        Created = date,
    };
}
