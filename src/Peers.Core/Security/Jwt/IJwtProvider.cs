using System.Security.Claims;

namespace Peers.Core.Security.Jwt;

/// <summary>
/// Base contract for JWT generation.
/// </summary>
public interface IJwtProvider
{
    /// <summary>
    /// Creates a new JWT for the specified user.
    /// </summary>
    /// <param name="userRoles">The user roles.</param>
    /// <param name="userClaims">The user claims.</param>
    /// <param name="expireDate">An optional token expiry date.</param>
    /// <returns></returns>
    (string, DateTime) BuildToken(IEnumerable<string> userRoles, IEnumerable<Claim> userClaims, DateTime? expireDate = null);
}
