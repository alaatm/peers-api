using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Peers.Core.Identity;

namespace Peers.Core.Security.Jwt;

/// <summary>
/// Base class for JWT generation.
/// </summary>
public sealed class JwtProvider : IJwtProvider
{
    private readonly TimeProvider _timeProvider;
    private readonly JwtConfig _config;

    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly SigningCredentials _signingCredentials;

    public JwtProvider(TimeProvider timeProvider, JwtConfig config)
    {
        _timeProvider = timeProvider;
        _config = config;

        // Disable inbound/outbound claims mapping globally and on this instance.
        // Note we still need to set MapInboundClaims to false in the JwtBearerOptions in AddJwtBearer call

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false,
            SetDefaultTimesOnTokenCreation = false,
        };
        _jwtSecurityTokenHandler.InboundClaimTypeMap.Clear();
        _jwtSecurityTokenHandler.OutboundClaimTypeMap.Clear();

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(_config.KeyBytes),
            SecurityAlgorithms.HmacSha256Signature);
    }

    /// <summary>
    /// Creates a new JWT for the specified user.
    /// </summary>
    /// <param name="userRoles">The user roles.</param>
    /// <param name="userClaims">The user claims.</param>
    /// <param name="expireDate">An optional token expiry date.</param>
    /// <returns></returns>
    public (string, DateTime) BuildToken(
        [NotNull] IEnumerable<string> userRoles,
        [NotNull] IEnumerable<Claim> userClaims,
        DateTime? expireDate = null)
    {
        var now = _timeProvider.UtcNow();
        var expires = expireDate ?? now.AddMinutes(_config.DurationInMinutes);

        var isNonEnumRoles = userRoles.TryGetNonEnumeratedCount(out var roleCount);
        var isNonEnumClaims = userClaims.TryGetNonEnumeratedCount(out var claimCount);
        Debug.Assert(isNonEnumRoles && isNonEnumClaims, "Enumerated roles/claims should not be passed to this method.");

        var claims = new List<Claim>(roleCount + claimCount);
        claims.AddRange(userClaims);
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(CustomClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);

        var token = _jwtSecurityTokenHandler.CreateJwtSecurityToken(
            issuer: _config.Issuer,
            subject: identity,
            expires: expires,
            issuedAt: now,
            signingCredentials: _signingCredentials);

        return (_jwtSecurityTokenHandler.WriteToken(token), expires);
    }
}
