namespace Mashkoor.Modules.Users.Domain;

/// <summary>
/// Represents a system notification
/// </summary>
public sealed class Notification : Entity
{
    /// <summary>
    /// The date/time at which the notification was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }
    /// <summary>
    /// The notification creator.
    /// </summary>
    public string CreatedBy { get; set; } = default!;
    /// <summary>
    /// The notification contents.
    /// </summary>
    public string Contents { get; set; } = default!;

    /// <summary>
    /// Creates a new instance of <see cref="Notification"/>.
    /// </summary>
    /// <param name="date">The date of the notification.</param>
    /// <param name="contents">The notification contents.</param>
    /// <param name="createdBy">The notification creator.</param>
    /// <returns></returns>
    internal static Notification Create(
        DateTime date,
        string contents,
        string? createdBy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(contents));

        return new()
        {
            CreatedOn = date,
            CreatedBy = createdBy ?? "SYSTEM",
            Contents = contents,
        };
    }
}
