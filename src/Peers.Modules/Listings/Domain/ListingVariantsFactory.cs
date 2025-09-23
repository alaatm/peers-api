using System.Diagnostics;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Provides factory methods for generating all possible combinations of listing variants based on attribute definitions
/// and their options.
/// </summary>
internal static class ListingVariantsFactory
{
    /// <summary>
    /// Generates all possible combinations of listing variants based on the provided attribute definitions and their options.
    /// All attribute definitions are assumed to be of enum type (i.e., they have predefined options) and are marked as variant attributes.
    /// Input dictionary is assumed to be non-empty.
    /// </summary>
    /// <param name="listing">The listing for which the variants are being generated.</param>
    /// <param name="axes">The input dictionary where each key is an attribute definition and the corresponding value is a list of its possible options.</param>
    /// <returns></returns>
    public static List<ListingVariant> GenerateVariants(
        Listing listing,
        Dictionary<EnumAttributeDefinition, List<EnumAttributeOption>> axes)
    {
        Debug.Assert(axes.Count > 0, "At least one variant axis is required.");

        var variants = new List<ListingVariant>();

        foreach (var combo in Cartesian())
        {
            variants.Add(ListingVariant.Create(listing, combo));
        }

        return variants;

        List<List<(AttributeDefinition def, EnumAttributeOption opt)>> Cartesian()
        {
            var combos = new List<List<(AttributeDefinition def, EnumAttributeOption opt)>> { new() };

            foreach (var (def, opts) in axes.OrderBy(x => x.Key.Position))
            {
                var orderedOpts = opts.OrderBy(o => o.Position).ToArray();
                var newCombos = new List<List<(AttributeDefinition def, EnumAttributeOption opt)>>(combos.Count * Math.Max(1, opts.Count));

                foreach (var combo in combos)
                {
                    foreach (var opt in orderedOpts)
                    {
                        var newCombo = new List<(AttributeDefinition def, EnumAttributeOption opt)>(combo) { (def, opt) };
                        newCombos.Add(newCombo);
                    }
                }
                combos = newCombos;
            }

            return combos;
        }
    }
}
