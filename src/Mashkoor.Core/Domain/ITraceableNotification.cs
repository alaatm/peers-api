using System.Diagnostics.CodeAnalysis;
using Mashkoor.Core.Identity;
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

public abstract class TraceableNotification : ITraceableNotification
{
    public string? TraceIdentifier { get; }

    protected TraceableNotification([NotNull] IIdentityInfo identityInfo) => TraceIdentifier = identityInfo.TraceIdentifier;
}
