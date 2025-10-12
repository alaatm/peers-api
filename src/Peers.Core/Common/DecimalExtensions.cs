using System.Globalization;

namespace Peers.Core.Common;

public static class DecimalExtensions
{
    extension(decimal value)
    {
        /// <summary>
        /// Returns the number of decimal places in the decimal value.
        /// </summary>
        /// <returns></returns>
        public int GetDecimalPlaces()
        {
            var count = 0;
            while (value != Math.Floor(value))
            {
                count++;
                value *= 10;
            }
            return count;
        }

        /// <summary>
        /// Returns a normalized string representation of the value, omitting unnecessary trailing zeros and using
        /// invariant culture formatting.
        /// </summary>
        public string Normalize()
            => value.ToString("G29", CultureInfo.InvariantCulture);
    }
}
