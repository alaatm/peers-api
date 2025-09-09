namespace Peers.Modules.Users.Domain;

/// <summary>
/// Represents the join entity for user and notification.
/// </summary>
public sealed class UserNotification : Entity
{
    /// <summary>
    /// The user which is targeted by the notification.
    /// </summary>
    public AppUser User { get; set; } = default!;
    /// <summary>
    /// The notification.
    /// </summary>
    public Notification Notification { get; set; } = default!;
    /// <summary>
    /// Indicates whether the notification has been read or not.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="UserNotification"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="notification">The notification.</param>
    /// <returns></returns>
    internal static UserNotification Create(AppUser user, Notification notification)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(notification);

        return new()
        {
            User = user,
            Notification = notification,
        };
    }
}
