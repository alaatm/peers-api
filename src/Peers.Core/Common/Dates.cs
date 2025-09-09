using System.Diagnostics;

namespace Peers.Core.Common;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo _uaeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");

    /// <summary>
    /// Converts the <see cref="DateTime"/> object to a <see cref="DateOnly"/> object.
    /// </summary>
    /// <param name="dateTime">The date to convert.</param>
    /// <returns></returns>
    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    /// <summary>
    /// Converts the nullable <see cref="DateTime"/> object to a nullable <see cref="DateOnly"/> object.
    /// </summary>
    /// <param name="dateTime">The date to convert.</param>
    /// <returns></returns>
    public static DateOnly? ToDateOnly(this DateTime? dateTime) => dateTime is null
        ? null
        : DateOnly.FromDateTime(dateTime.Value);

    /// <summary>
    /// Converts the given <see cref="DateTime"/> to Saudi Arabia time zone.
    /// </summary>
    /// <param name="dateTime">The UTC date/time.</param>
    /// <returns></returns>
    public static DateTime ToUaeTimeZone(this DateTime dateTime)
    {
        Debug.Assert(dateTime.Kind == DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _uaeTimeZone);
    }
    /// <summary>
    /// Converts the given <see cref="DateTime"/> to Saudi Arabia time zone.
    /// </summary>
    /// <param name="dateTime">The UTC date/time.</param>
    /// <returns></returns>
    public static DateTimeOffset ToUaeTimeZoneOffset(this DateTime dateTime)
    {
        Debug.Assert(dateTime.Kind == DateTimeKind.Utc);
        DateTimeOffset dateTimeOffset = dateTime;
        return TimeZoneInfo.ConvertTime(dateTimeOffset, _uaeTimeZone);
    }
}
