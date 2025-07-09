namespace Mashkoor.Modules.Users.Domain;

/// <summary>
/// Represents a single usage of the app.
/// </summary>
/// <param name="OpenedAt">The date/time when the app was opened.</param>
/// <returns></returns>
public sealed record AppUsageHistory(DateTime OpenedAt);
