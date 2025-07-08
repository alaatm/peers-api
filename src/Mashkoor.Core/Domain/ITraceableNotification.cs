using MediatR;

namespace Mashkoor.Core.Domain;

/// <summary>
/// Represents a traceable notification.
/// </summary>
public interface ITraceableNotification : INotification
{
    /// <summary>
    /// The trace id.
    /// </summary>
    string? TraceIdentifier { get; }
}
