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
    }
}
