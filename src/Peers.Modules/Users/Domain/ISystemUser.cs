namespace Peers.Modules.Users.Domain;

/// <summary>
/// Represents a generic system user.
/// </summary>
public interface ISystemUser
{
    /// <summary>
    /// The user id.
    /// </summary>
    /// <remarks>
    /// This is the same as the <see cref="AppUser"/> Id property. It will be the same across all the user types.
    /// </remarks>
    int Id { get; set; }
    /// <summary>
    /// The username.
    /// </summary>
    string Username { get; }
    /// <summary>
    /// The linked database user.
    /// </summary>
    AppUser User { get; }
}
