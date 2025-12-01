namespace Peers.Modules.Users.Commands.Responses;

/// <summary>
/// Represents a JWT response containing authentication details.
/// </summary>
/// <param name="Username">The username of the authenticated user.</param>
/// <param name="Token">The JWT token.</param>
/// <param name="RefreshToken">The refresh token. Use this token to obtain new JWT tokens without requiring the user to re-authenticate.</param>
/// <param name="TokenExpiry">The expiration time of the JWT.</param>
/// <param name="Roles">The roles assigned to the user.</param>
public sealed record JwtResponse(
    string Username,
    string Token,
    string RefreshToken,
    DateTime TokenExpiry,
    string[] Roles);
