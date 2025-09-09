namespace Peers.Core.Identity;

/// <summary>
/// Contract for an application user identity.
/// </summary>
public interface IIdentityInfo
{
    /// <summary>
    /// The request's trace identifier.
    /// </summary>
    string? TraceIdentifier { get; }
    /// <summary>
    /// Gets a value indicating user IP
    /// </summary>
    string? Ip { get; }
    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
    /// <summary>
    /// Gets the user id. Returns -1 if the user is not authenticated or no context.
    /// </summary>
    int Id { get; }
    /// <summary>
    /// Gets the username.
    /// </summary>
    string? Username { get; }
    /// <summary>
    /// Determines whether the user is in the specified role.
    /// </summary>
    /// <param name="role">The role.</param>
    bool IsInRole(string role);
}
