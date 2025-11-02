using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Common;

/// <summary>
/// Provides extension methods for working with generic lists.
/// </summary>
public static class ListExtensions
{
    extension<T>(List<T> source)
    {
        /// <summary>
        /// Determines whether the source sequence contains any duplicate elements and returns the first duplicate
        /// found.
        /// </summary>
        /// <remarks>The method compares elements using the default equality comparer for type T. If
        /// multiple duplicates exist, only the first encountered duplicate is returned in the out parameter.</remarks>
        /// <param name="duplicate">When this method returns, contains the first duplicate element found in the source sequence, if any;
        /// otherwise, the default value for type T.</param>
        /// <returns>true if a duplicate element is found in the source sequence; otherwise, false.</returns>
        public bool HasDuplicates([MaybeNullWhen(false)] out T duplicate)
        {
            var seen = new HashSet<T>();
            foreach (var item in source)
            {
                if (!seen.Add(item))
                {
                    duplicate = item;
                    return true;
                }
            }

            duplicate = default;
            return false;
        }
    }
}
