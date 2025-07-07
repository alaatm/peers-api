using System.Globalization;

namespace Mashkoor.Core.Identity;

/// <summary>
/// Represents an application user identity.
/// </summary>
public sealed class IdentityInfo : IIdentityInfo
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private HttpContext? Context => _httpContextAccessor.HttpContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityInfo"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public IdentityInfo(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// The request's trace identifier.
    /// </summary>
    public string? TraceIdentifier => Context?.TraceIdentifier ?? null;
    /// <summary>
    /// Gets a value indicating user IP
    /// </summary>
    public string? Ip => Context?.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? null;
    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated => Context?.User.Identity?.IsAuthenticated ?? false;
    /// <summary>
    /// Gets the user id.
    /// </summary>
    public int Id
    {
        get
        {
            if (field is 0 && GetClaimValue(CustomClaimTypes.Id) is string idString)
            {
                field = int.Parse(idString, CultureInfo.InvariantCulture);
            }

            return field;
        }
    }
    /// <summary>
    /// Gets the username.
    /// </summary>
    public string? Username => Context?.User.Identity?.Name ?? null;

    /// <summary>
    /// Determines whether the user is in the specified role.
    /// </summary>
    /// <param name="role">The role.</param>
    public bool IsInRole(string role) => Context?.User.IsInRole(role) ?? false;

    private string? GetClaimValue(string type)
    {
        if (Context is not null)
        {
            foreach (var claim in Context.User.Claims)
            {
                if (claim.Type == type)
                {
                    return claim.Value;
                }
            }
        }

        return null;
    }
}
