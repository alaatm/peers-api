namespace Peers.Modules.Users.Domain;

/// <summary>
/// Represents a user ban status change entry.
/// </summary>
public sealed class UserStatusChange : Entity
{
    /// <summary>
    /// The date/time of the change.
    /// </summary>
    public DateTime ChangedOn { get; set; }
    /// <summary>
    /// The authorized staff who performed the change.
    /// </summary>
    public AppUser ChangedBy { get; set; } = default!;
    /// <summary>
    /// The old status.
    /// </summary>
    public UserStatus OldStatus { get; set; }
    /// <summary>
    /// The new status.
    /// </summary>
    public UserStatus NewStatus { get; set; }
    /// <summary>
    /// The change reason.
    /// </summary>
    public string ChangeReason { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of the class.
    /// </summary>
    /// <param name="date">When the change occurred.</param>
    /// <param name="changedBy">The staff who is performing the change.</param>
    /// <param name="oldStatus">The old status.</param>
    /// <param name="newStatus">The new status.</param>
    /// <param name="changeReason">The change reason.</param>
    internal static UserStatusChange Create(
        DateTime date,
        AppUser changedBy,
        UserStatus oldStatus,
        UserStatus newStatus,
        string changeReason)
    {
        ArgumentNullException.ThrowIfNull(changedBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(changeReason));

        return new UserStatusChange
        {
            ChangedOn = date,
            ChangedBy = changedBy,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangeReason = changeReason
        };
    }
}
