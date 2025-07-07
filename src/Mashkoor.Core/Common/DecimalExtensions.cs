namespace Mashkoor.Core.Common;

/// <summary>
/// Provides extension methods for <see cref="decimal"/> values.
/// </summary>
public static class DecimalExtensions
{
    /// <summary>
    /// Returns the number of decimal places in the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to get decimal places count for.</param>
    /// <returns></returns>
    public static int GetDecimalPlaces(this decimal value)
    {
        var count = 0;
        while (value != Math.Floor(value))
        {
            count++;
            value *= 10;
        }
        return count;
    }
}
