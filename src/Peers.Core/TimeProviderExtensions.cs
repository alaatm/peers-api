using System.Diagnostics.CodeAnalysis;
using Peers.Core.Common;

namespace Peers.Core;

/// <summary>
/// Provides extension methods for <see cref="TimeProvider"/> class.
/// </summary>
public static class TimeProviderExtensions
{
    /// <inheritdoc cref="DateTime.UtcNow"/>
    public static DateTime UtcNow([NotNull] this TimeProvider timeProvider)
        => timeProvider.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Gets a <see cref="DateOnly"/> object that is set to the current date on this
    /// computer, expressed as the Coordinated Universal Time (UTC).
    /// </summary>
    public static DateOnly UtcToday([NotNull] this TimeProvider timeProvider)
        => timeProvider.UtcNow().ToDateOnly();
}
