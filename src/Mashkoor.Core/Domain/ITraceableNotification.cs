using System.Diagnostics.CodeAnalysis;
using MediatR;
using Mashkoor.Core.Identity;

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

public abstract class TenantTraceableNotificationBase : ITraceableNotification
{
    public string? TraceIdentifier { get; }

    protected TenantTraceableNotificationBase([NotNull] IIdentityInfo identityInfo)
        => TraceIdentifier = identityInfo.TraceIdentifier;
}
