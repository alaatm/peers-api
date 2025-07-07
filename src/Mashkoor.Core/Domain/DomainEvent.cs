namespace Mashkoor.Core.Domain;

/// <summary>
/// Represents an abstract domain event.
/// </summary>
public abstract class DomainEvent : ITraceableNotification
{
    /// <summary>
    /// The trace id.
    /// </summary>
    public string? TraceIdentifier { get; set; }
    /// <summary>
    /// The date/time at which this event has occurred.
    /// </summary>
    public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}
