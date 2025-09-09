using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Peers.Core.Data.ValueConverters;

/// <summary>
/// Converts <see cref="DateTime" /> to <see cref="DateTime"/> with a UTC kind.
/// </summary>
public sealed class DateTimeUTCConverter : ValueConverter<DateTime, DateTime>
{
    /// <summary>
    /// Creates a new instance of this converter.
    /// </summary>
    public DateTimeUTCConverter() : base(
            d => d,
            d => new DateTime(d.Ticks, DateTimeKind.Utc))
    { }
}

/// <summary>
/// Converts <see cref="Nullable{DateTime}" /> to <see cref="Nullable{DateTime}"/> with a UTC kind.
/// </summary>
public sealed class NullableDateTimeUTCConverter : ValueConverter<DateTime?, DateTime?>
{
    /// <summary>
    /// Creates a new instance of this converter.
    /// </summary>
    public NullableDateTimeUTCConverter() : base(
        d => d,
        d => d == null
            ? null
            : new DateTime(d.Value.Ticks, DateTimeKind.Utc))
    { }
}
