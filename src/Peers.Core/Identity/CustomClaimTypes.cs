namespace Peers.Core.Identity;

/// <summary>
/// Represents custom identity claim types.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// The user identifier claim type.
    /// </summary>
    public const string Id = "id";
    /// <summary>
    /// The user role claim type.
    /// </summary>
    public const string Role = "rl";
    /// <summary>
    /// The username claim type. Used for The NameClaimType in JWT validation.
    /// </summary>
    public const string Username = "un";
}
