namespace Mashkoor.Modules.Users.Domain;

/// <summary>
/// Represents a system user status.
/// </summary>
/// <remarks>
/// Theses can only be changes by authorized staff members.
/// </remarks>
public enum UserStatus
{
    /// <summary>
    /// The default value for this enum which is also invalid for a user.
    /// </summary>
    None,
    /// <summary>
    /// This is the initial status when a new user is created,
    /// where all tasks can be performed.
    /// </summary>
    Active,
    /// <summary>
    /// A user can perform minimal functions within the system,
    /// depending on the user time.
    /// </summary>
    Suspended,
    /// <summary>
    /// Banned user cannot perform any tasks.
    /// </summary>
    Banned,
    /// <summary>
    /// A user account that has been deleted.
    /// </summary>
    Deleted,
}
