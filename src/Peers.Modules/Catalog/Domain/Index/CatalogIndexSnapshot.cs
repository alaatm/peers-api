using System.Diagnostics;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Domain.Index;

public sealed record CatalogIndexSnapshot(
    // attrKey -> [ optsCodes ]
    Dictionary<string, HashSet<string>> EnumByCode,
    // attrKey -> [ optsCodes ]
    Dictionary<string, HashSet<string>> LookupByCode,
    // parentAttrKey -> childAttrKey -> parentOptCode -> [ childOptsCodes ]
    Dictionary<string, Dictionary<string, Dictionary<string, HashSet<string>>>> EnumDeps,
    // parentAttrKey -> childAttrKey -> parentOptCode -> [ childOptsCodes ]
    Dictionary<string, Dictionary<string, Dictionary<string, HashSet<string>>>> LookupDeps,
    // attrKey -> [ allowedOptsCodes ] (empty means allow all by default unless overriden by caller)
    Dictionary<string, HashSet<string>> LookupAllowed)
{
    internal static CatalogIndexSnapshot Build(ProductType pt)
    {
        // 1) Current PT attributes only (no inherited defs)
        var enumAttrs = pt.Attributes.OfType<EnumAttributeDefinition>().ToArray();
        var lookupAttrs = pt.Attributes.OfType<LookupAttributeDefinition>().ToArray();

        // 2) Option maps (by current PT attribute key)
        var enumByCode = enumAttrs.ToDictionary(
            a => a.Key,
            a => a.Options.Select(o => o.Code).ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);

        var lookupByCode = lookupAttrs.ToDictionary(
            a => a.Key,
            a => a.LookupType.Options.Select(o => o.Code).ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);

        // 2) Enum→Enum deps from child options' ParentOptionId
        var enumDeps = new Dictionary<string, Dictionary<string, Dictionary<string, HashSet<string>>>>(StringComparer.Ordinal);
        foreach (var child in enumAttrs)
        {
            if (child.DependsOn is not EnumAttributeDefinition parent)
            {
                continue;
            }

            var childMap = enumDeps
                .GetOrAdd(parent.Key, _ => new(StringComparer.Ordinal))
                .GetOrAdd(child.Key, _ => new(StringComparer.Ordinal));

            // parentCode from parent's options; childCode from child's options
            // assumes child.Options[i].ParentOptionId points to parent.Options[].Id
            var parentToCode = parent.Options.ToDictionary(o => o, o => o.Code);
            foreach (var opt in child.Options)
            {
                Debug.Assert(opt.ParentOption is not null);
                var parentCode = parentToCode[opt.ParentOption];
                childMap.GetOrAdd(parentCode, _ => []).Add(opt.Code);
            }
        }

        // 3) Map lookup type -> attribute keys on THIS ProductType
        // (multiple attrs may share the same lookup type)
        var lookupDeps = new Dictionary<string, Dictionary<string, Dictionary<string, HashSet<string>>>>(StringComparer.Ordinal);

        var parentTypeToAttrKeys = lookupAttrs
            .GroupBy(a => a.LookupType)
            .ToDictionary(g => g.Key, g => g.Select(a => a.Key).ToArray());

        // Child type id -> child attribute key (1:1 on this ProductType)
        var childTypeToAttrKey = lookupAttrs
            .ToDictionary(a => a.LookupType, a => a.Key);

        // For each lookup TYPE used on this PT as a potential PARENT...
        // (We materialize types once via the attrs; ParentLinks must be loaded)
        var allParentTypes = lookupAttrs
            .Select(a => a.LookupType)
            .DistinctBy(t => t.Id)
            .ToArray();

        foreach (var parentType in allParentTypes)
        {
            // Which parent attribute keys on this PT use this lookup type?
            if (!parentTypeToAttrKeys.TryGetValue(parentType, out var parentAttrKeys))
            {
                continue;
            }

            // Group links by CHILD lookup type id
            // (ParentLinks must have ParentOption & ChildOption loaded)
            var groups = parentType.ParentLinks.GroupBy(l => l.ChildType);

            foreach (var grp in groups)
            {
                // Is that child type represented by *an attribute* on this PT?
                if (!childTypeToAttrKey.TryGetValue(grp.Key, out var childAttrKey))
                {
                    continue;
                }

                // For EACH parent attribute key (fan-out if multiple attrs share the same type)
                foreach (var parentAttrKey in parentAttrKeys)
                {
                    var byChildAttr = lookupDeps.GetOrAdd(parentAttrKey, _ => new(StringComparer.Ordinal));
                    var byParentCode = byChildAttr.GetOrAdd(childAttrKey, _ => new(StringComparer.Ordinal));

                    // Fold concrete option-to-option links
                    foreach (var link in grp)
                    {
                        var pCode = link.ParentOption.Code;
                        var cCode = link.ChildOption.Code;

                        byParentCode.GetOrAdd(pCode, _ => new(StringComparer.Ordinal))
                                    .Add(cCode);
                    }
                }
            }
        }

        // 4) Lookup allow-lists per attribute key
        var lookupAllow = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var attr in lookupAttrs)
        {
            Debug.Assert(
                attr.AllowedOptions.Count > 0 ||
                (attr.AllowedOptions.Count == 0 && attr.LookupType.ConstraintMode is LookupConstraintMode.Open));

            lookupAllow[attr.Key] = [.. attr.AllowedOptions.Select(o => o.Option.Code)];
        }

        return new CatalogIndexSnapshot
        (
            enumByCode,
            lookupByCode,
            enumDeps,
            lookupDeps,
            lookupAllow
        );
    }

    internal CatalogIndex Hydrate(ProductType pt)
    {
        var defsByKey = pt.Attributes.ToDictionary(
            p => p.Key,
            p => p,
            StringComparer.Ordinal);

        return new()
        {
            DefsByKey = defsByKey,
            EnumByCode = EnumByCode.ToDictionary(
            p => p.Key,
            p => ((EnumAttributeDefinition)defsByKey[p.Key])
                .Options
                .Where(o => p.Value.Contains(o.Code))
                .ToDictionary(o => o.Code, o => o),
                StringComparer.Ordinal),
            LookupByCode = LookupByCode.ToDictionary(
            p => p.Key,
            p => ((LookupAttributeDefinition)defsByKey[p.Key])
                .LookupType
                .Options
                .Where(o => p.Value.Contains(o.Code))
                .ToDictionary(o => o.Code, o => o),
            StringComparer.Ordinal),
            EnumDeps = EnumDeps,
            LookupDeps = LookupDeps,
            LookupAllowed = LookupAllowed,
        };
    }
}
