using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Lookup.Domain;
using static Peers.Modules.Listings.Commands.SetAttributes.Command;

namespace Peers.Modules.Catalog.Domain.Index;

public sealed class CatalogIndex
{
    private Dictionary<string, AttributeInputDto>? _inputs;
    private Dictionary<string, string>? _nonVariantCodes;

    // attrKey -> AttributeDefinition
    internal Dictionary<string, AttributeDefinition> DefsByKey { get; init; } =
        new(StringComparer.Ordinal);

    // attrKey -> optCode -> EnumAttributeOption
    internal Dictionary<string, Dictionary<string, EnumAttributeOption>> EnumByCode { get; init; } =
        new(StringComparer.Ordinal);

    // attrKey -> optCode -> LookupOption
    internal Dictionary<string, Dictionary<string, LookupOption>> LookupByCode { get; init; } =
        new(StringComparer.Ordinal);

    // parentAttrKey -> childAttrKey -> parentOptCode -> [ childOptsCodes ]
    internal Dictionary<string, Dictionary<string, Dictionary<string, HashSet<string>>>> EnumDeps { get; init; } =
        new(StringComparer.Ordinal);

    // parentAttrKey -> childAttrKey -> parentOptCode -> [ childOptsCodes ]
    internal Dictionary<string, Dictionary<string, Dictionary<string, HashSet<string>>>> LookupDeps { get; init; } =
        new(StringComparer.Ordinal);

    // attrKey -> [ allowedOptsCodes ] (empty means allow all by default unless overriden by caller)
    internal Dictionary<string, HashSet<string>> LookupAllowed { get; init; } =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Attempts to retrieve the attribute definition associated with the specified key.
    /// </summary>
    /// <param name="attrKey">The key of the attribute definition to locate.</param>
    /// <param name="def">When this method returns, contains the attribute definition associated with the specified key, if the key is
    /// found; otherwise, null.</param>
    /// <returns>true if the attribute definition was found; otherwise, false.</returns>
    internal bool TryGetDefinition(
        string attrKey,
        [NotNullWhen(true)] out AttributeDefinition? def)
        => DefsByKey.TryGetValue(attrKey, out def);

    /// <summary>
    /// Attempts to retrieve the enum attribute option associated with the specified attribute key and code.
    /// </summary>
    /// <param name="attrKey">The key that identifies the enum attribute whose option is to be retrieved.</param>
    /// <param name="code">The code corresponding to the desired enum option.</param>
    /// <param name="opt">When this method returns, contains the matching <see cref="EnumAttributeOption"/> if found; otherwise, null.</param>
    /// <returns>true if an enum option matching the specified attribute key and code is found; otherwise, false.</returns>
    internal bool TryGetEnumOption(
        string attrKey,
        string code,
        [NotNullWhen(true)] out EnumAttributeOption? opt)
    {
        opt = null;
        return
            EnumByCode.TryGetValue(attrKey, out var map) &&
            map.TryGetValue(code, out opt);
    }

    /// <summary>
    /// Determines whether an enum dependency (dense) exists between the specified parent and child enum attribute.
    /// </summary>
    /// <param name="parentAttrKey">The key of the parent enum attribute.</param>
    /// <param name="childAttrKey">The key of the child enum attribute.</param>
    /// <returns>true if a dependency exists between the specified parent and child enum attributes; otherwise, false.</returns>
    internal bool HasEnumDependency(
        string parentAttrKey,
        string childAttrKey) =>
        EnumDeps.TryGetValue(parentAttrKey, out var byChild) &&
        byChild.ContainsKey(childAttrKey);

    /// <summary>
    /// For a declared pair, attempts to retrieve the allowed child codes for a specific parent code.
    /// Returns true if a row exists for that parentCode (dense row required).
    /// </summary>
    /// <param name="parentAttrKey">The key of the parent enum attribute to query.</param>
    /// <param name="childAttrKey">The key of the child enum attribute whose allowed options are to be retrieved.</param>
    /// <param name="parentCode">The code representing the parent option for which allowed child options are requested.</param>
    /// <param name="allowed">When this method returns true, contains a set of allowed child options for the specified
    /// parent; otherwise, false.</param>
    /// <returns>true if the allowed child values were found for the specified parent attribute, child
    /// attribute, and parent code; otherwise, false.</returns>
    internal bool TryGetEnumAllowedChildren(
        string parentAttrKey,
        string childAttrKey,
        string parentCode,
        [NotNullWhen(true)] out HashSet<string>? allowed)
    {
        allowed = null;
        return EnumDeps.TryGetValue(parentAttrKey, out var byChild)
            && byChild.TryGetValue(childAttrKey, out var byParent)
            && byParent.TryGetValue(parentCode, out allowed);
    }

    /// <summary>
    /// Attempts to retrieve the lookup attribute option associated with the specified attribute key and code.
    /// </summary>
    /// <param name="attrKey">The key identifying the lookup attribute whose option is to be retrieved.</param>
    /// <param name="code">The code corresponding to the desired lookup option.</param>
    /// <param name="opt">When this method returns, contains the matching <see cref="LookupOption"/> if found; otherwise, null.</param>
    /// <returns>true if a matching lookup option is found; otherwise, false.</returns>
    internal bool TryGetLookupOption(
        string attrKey,
        string code,
        [NotNullWhen(true)] out LookupOption? opt)
    {
        opt = null;
        return
            LookupByCode.TryGetValue(attrKey, out var map) &&
            map.TryGetValue(code, out opt);
    }

    /// <summary>
    /// Determines whether a lookup dependency (dense pair + allow-list filtered) exists between the specified parent and child lookup attribute.
    /// </summary>
    /// <param name="parentAttrKey">The key of the parent lookup attribute.</param>
    /// <param name="childAttrKey">The key of the child lookup attribute.</param>
    /// <returns>true if a dependency exists between the specified parent and child attributes; otherwise, false.</returns>
    public bool HasLookupDependency(
        string parentAttrKey,
        string childAttrKey) =>
        LookupDeps.TryGetValue(parentAttrKey, out var byChild) &&
        byChild.ContainsKey(childAttrKey);

    /// <summary>
    /// Attempts to retrieve the lookup option for the specified attribute key and code, considering the current allow-list policy.
    /// i.e., Existence + allow-list in one go (empty allow-list ⇒ allow-all unless caller overrides)
    /// </summary>
    /// <param name="attrKey">The key of the lookup attribute whose option is to be resolved.</param>
    /// <param name="code">The code corresponding to the desired lookup option.</param>
    /// <param name="noEntriesMeansAllowAll">Determines whether all codes are considered allowed when the allow-list for the attribute key is empty. Set to
    /// true to allow all codes in this case; otherwise, false.</param>
    /// <returns>true if the lookup option is resolved and allowed by the current policy; otherwise, false.</returns>
    /// <exception cref="InvalidDomainStateException">Thrown if the specified attribute key does not exist in the allow-list, indicating an invalid domain state.</exception>
    internal bool IsLookupOptionAllowed(
        string attrKey,
        string code,
        bool noEntriesMeansAllowAll = true)
    {
        // Allow-list must exist, enforce it.
        if (!LookupAllowed.TryGetValue(attrKey, out var allowSet))
        {
            // Invariant breach
            throw new InvalidDomainStateException($"Lookup attribute '{attrKey}' not found in CatalogIndex allow-list.");
        }

        return allowSet.Count == 0
            ? noEntriesMeansAllowAll
            : allowSet.Contains(code);
    }

    /// <summary>
    /// For a declared pair, gets the allowed child codes for a specific parent code, intersected with the child's per-attribute allow-list.
    /// Returns true if a row exists for that parentCode, i.e. an explicit constraint set for (parentAttrKey, childAttrKey, parentCode). (dense row required).
    /// When it returns false, treat as "no constraint from this parent option".
    /// </summary>
    /// <remarks>Returns false if there is no explicit constraint for the given parent and child attribute
    /// keys and parent code. In this case, the caller should treat the result as indicating no constraint from the
    /// parent option.</remarks>
    /// <param name="parentAttrKey">The key of the parent lookup attribute.</param>
    /// <param name="childAttrKey">The key of the child lookup attribute.</param>
    /// <param name="parentCode">The code representing the specific parent option for which allowed child codes are requested.</param>
    /// <param name="allowSet">When this method returns, contains the set of allowed child codes for the specified parent code and attribute
    /// pair, filtered by the child's allow-list. The set is empty if no allowed codes are found.</param>
    /// <param name="noEntriesMeansAllowAll">A value indicating whether an empty allow-list for the child attribute should be interpreted as allowing all
    /// codes. If set to true, all codes from the dependency are allowed when the allow-list is empty;
    /// otherwise, no codes are allowed.</param>
    /// <returns>true if an explicit constraint exists for the specified parent and child attribute keys and parent code, and at
    /// least one allowed child code is found; otherwise, false.</returns>
    internal bool TryGetLookupAllowedChildren(
        string parentAttrKey,
        string childAttrKey,
        string parentCode,
        out HashSet<string> allowSet,
        bool noEntriesMeansAllowAll = true)
    {
        allowSet = new(StringComparer.Ordinal);

        if (!LookupDeps.TryGetValue(parentAttrKey, out var byChild) ||
            !byChild.TryGetValue(childAttrKey, out var byParent))
        {
            // Pair not declared, caller treats as "no constraint"
            return false;
        }

        if (!byParent.TryGetValue(parentCode, out var allowedByParent))
        {
            // Dense row missing, invalid under dense policy
            return false;
        }

        if (!LookupAllowed.TryGetValue(childAttrKey, out var allowedByChild))
        {
            throw new InvalidDomainStateException($"Lookup attribute '{childAttrKey}' not found in CatalogIndex allow-list.");
        }

        if (allowedByChild.Count == 0)
        {
            if (!noEntriesMeansAllowAll)
            {
                return false;
            }

            allowSet = new HashSet<string>(allowedByParent, StringComparer.Ordinal);
            return allowSet.Count > 0;
        }

        foreach (var c in allowedByParent)
        {
            if (allowedByChild.Contains(c))
            {
                allowSet.Add(c);
            }
        }

        return allowSet.Count > 0;
    }

    internal void InitializeListingValidationWithInputs(Dictionary<string, AttributeInputDto> inputs)
    {
        _inputs = inputs;
        _nonVariantCodes = [];
        foreach (var (key, input) in inputs)
        {
            if (input is AttributeInputDto.OptionCodeOrScalarString scalar)
            {
                _nonVariantCodes[key] = scalar.Value;
            }
        }
    }

    /// <summary>
    /// Validate a concrete selection (variant + non-variant) against all enum and lookup deps.
    /// </summary>
    /// <param name="pick">The list of selected variant axes and their chosen options. Represents the selected values for variant attributes in the combination.</param>
    /// <param name="noEntriesMeansAllowAll">true to allow all combinations when no dependency entries are defined; otherwise, false to require explicit
    /// entries for a combination to be considered valid. The default is true.</param>
    /// <returns>true if the combination of variant and non-variant codes is valid according to the defined dependencies;
    /// otherwise, false.</returns>
    internal bool IsVariantComboValid(
        List<AxisSelection> pick,
        bool noEntriesMeansAllowAll = true)
    {
        var variantCodes = BuildVariantCodeMap(pick);
        var nonVariantCodes = _nonVariantCodes ?? throw new InvalidOperationException("Call InitializeListingValidationWithInputs() first");

        // Enum
        foreach (var (parentKey, byChild) in EnumDeps)
        {
            if (!TryGetSelected(variantCodes, nonVariantCodes, parentKey, out var parentCode))
            {
                continue;
            }

            foreach (var (childKey, byParent) in byChild)
            {
                if (!TryGetSelected(variantCodes, nonVariantCodes, childKey, out var childCode))
                {
                    continue;
                }

                // Dense policy for declared pair:
                if (!byParent.TryGetValue(parentCode, out var allowedChildren))
                {
                    return false;
                }

                if (!allowedChildren.Contains(childCode))
                {
                    return false;
                }
            }
        }

        // Lookup
        foreach (var (parentKey, byChild) in LookupDeps)
        {
            if (!TryGetSelected(variantCodes, nonVariantCodes, parentKey, out var parentCode))
            {
                continue;
            }

            foreach (var (childKey, _) in byChild)
            {
                if (!TryGetSelected(variantCodes, nonVariantCodes, childKey, out var childCode))
                {
                    continue;
                }

                // if pair exists, enforce densely.
                if (!TryGetLookupAllowedChildren(parentKey, childKey, parentCode, out var allowedChildCodes, noEntriesMeansAllowAll))
                {
                    // Pair declared but no row for this parent code which is invalid (dense)
                    return false;
                }

                if (!allowedChildCodes.Contains(childCode))
                {
                    return false;
                }
            }
        }

        return true;

        static Dictionary<string, string> BuildVariantCodeMap(List<AxisSelection> pick)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var sel in pick)
            {
                switch (sel.Definition)
                {
                    case EnumAttributeDefinition eDef when sel.Choice.EnumOption is not null:
                        map[eDef.Key] = sel.Choice.EnumOption!.Code;
                        break;

                    case LookupAttributeDefinition lDef when sel.Choice.LookupOption is not null:
                        map[lDef.Key] = sel.Choice.LookupOption!.Code;
                        break;

                    // Numeric/group members don't participate in enum/lookup dependency checks
                    default:
                        break;
                }
            }

            return map;
        }

        static bool TryGetSelected(
            IReadOnlyDictionary<string, string> v,
            IReadOnlyDictionary<string, string> nv,
            string key,
            [NotNullWhen(true)] out string? code)
        {
            if (v.TryGetValue(key, out code))
            {
                return true;
            }

            if (nv.TryGetValue(key, out code))
            {
                return true;
            }

            code = default!;
            return false;
        }
    }

    /// <summary>
    /// Given the offered parent codes (from variant axes or scalar selections), determines whether the specified child code is reachable
    /// through every declared dependency pair.
    /// </summary>
    /// <remarks>A child code is considered reachable only if, for every parent attribute that declares a
    /// dependency on the child, at least one of the provided parent codes allows the child code. If a parent attribute
    /// does not declare a dependency on the child, it does not affect reachability.</remarks>
    /// <param name="childAttrKey">The attribute key identifying the child for which reachability is being evaluated.</param>
    /// <param name="childCode">The option code of the child attribute to check for reachability.</param>
    /// <param name="noEntriesMeansAllowAll">true to allow all child codes if no explicit entries are found in a lookup dependency; otherwise, false.</param>
    /// <returns>true if the child code is reachable from the given parent codes according to all declared dependencies;
    /// otherwise, false.</returns>
    internal bool IsChildCodeReachableFromParents(
        string childAttrKey,
        string childCode,
        bool noEntriesMeansAllowAll = true)
    {
        var inputs = _inputs ?? throw new InvalidOperationException("Call InitializeListingValidationWithInputs() first");

        foreach (var (parentAttrKey, input) in inputs)
        {
            if (!HasEnumDependency(parentAttrKey, childAttrKey) &&
                !HasLookupDependency(parentAttrKey, childAttrKey))
            {
                // This parent doesn't gate the child
                continue;
            }

            var pCodes = input switch
            {
                AttributeInputDto.OptionCodeAxis axis => axis.Value,
                AttributeInputDto.OptionCodeOrScalarString scalar => [scalar.Value],
                _ => []
            };

            var reachable = false;
            foreach (var pCode in pCodes)
            {
                // Enum path
                if (HasEnumDependency(parentAttrKey, childAttrKey) &&
                    TryGetEnumAllowedChildren(parentAttrKey, childAttrKey, pCode, out var enumAllowed) &&
                    enumAllowed.Contains(childCode))
                {
                    reachable = true;
                    break;
                }

                // Lookup path (allow-list filtered)
                if (HasLookupDependency(parentAttrKey, childAttrKey) &&
                    TryGetLookupAllowedChildren(parentAttrKey, childAttrKey, pCode, out var lookupAllowed, noEntriesMeansAllowAll) &&
                    lookupAllowed.Contains(childCode))
                {
                    reachable = true;
                    break;
                }
            }

            if (!reachable)
            {
                // No parent code makes this child code valid
                return false;
            }
        }
        return true;
    }
}
