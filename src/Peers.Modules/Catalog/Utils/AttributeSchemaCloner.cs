using System.Diagnostics;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Utils;

/// <summary>
/// Provides functionality to clone attribute schema definitions and options from one product type to another.
/// </summary>
internal static class AttributeSchemaCloner
{
    /// <summary>
    /// Copies all attribute definitions and options from the specified source product type to the target product type.
    /// </summary>
    /// <remarks>This method performs a deep copy of attribute definitions, including dependencies and
    /// enumeration options, from the source to the target product type. The target product type must be empty before
    /// calling this method. Any existing attributes or options in the target will result in undefined behavior. The
    /// method preserves the dependency order and translations of attributes and options.</remarks>
    /// <param name="source">The product type from which attribute definitions and options are copied. Must not be null and must have the
    /// same kind as the target.</param>
    /// <param name="target">The product type to which attribute definitions and options are copied. Must not be null, must have the same
    /// kind as the source, and must not contain any attributes prior to the operation.</param>
    public static void CopyFrom(ProductType source, ProductType target)
    {
        Debug.Assert(!ReferenceEquals(source, target));
        Debug.Assert(source.Kind == target.Kind);
        Debug.Assert(target.Attributes.Count == 0);

        // Parents first (by DependsOn)
        var ordered = AttributeSchemaUtils.TopoOrderByDependency(source.Attributes);

        // Maps to resolve keys (safe because target is empty)
        var defMap = new Dictionary<string, AttributeDefinition>(StringComparer.Ordinal);
        var optMap = new Dictionary<(string defKey, string optCode), EnumAttributeOption>();

        foreach (var srcDef in ordered)
        {
            AttributeDefinition dstDef;

            // Enum with dependency
            if (srcDef is EnumAttributeDefinition srcDepDef &&
                srcDepDef.DependsOn is { } srcParentDepDef)
            {
                var dstParentDepDef = (EnumAttributeDefinition)defMap[srcParentDepDef.Key];

                dstDef = target.DefineDependentAttribute(
                    parentKey: dstParentDepDef.Key,
                    key: srcDepDef.Key,
                    isRequired: srcDepDef.IsRequired,
                    isVariant: (srcDef as EnumAttributeDefinition)?.IsVariant ?? false,
                    position: srcDepDef.Position);
            }
            // Everything else including Enum without dependency
            else
            {
                ExtractAttrDefConfig(srcDef,
                    out var unit,
                    out var min,
                    out var max,
                    out var step,
                    out var regex,
                    out var lookupType);

                dstDef = target.DefineAttribute(
                    key: srcDef.Key,
                    kind: srcDef.Kind,
                    isRequired: srcDef.IsRequired,
                    isVariant: srcDef.IsVariant,
                    position: srcDef.Position,
                    unit: unit,
                    min: min,
                    max: max,
                    step: step,
                    regex: regex,
                    lookupType: lookupType);
            }

            defMap[srcDef.Key] = dstDef;
            CopyDefinitionTranslations(srcDef, dstDef);

            if (srcDef is EnumAttributeDefinition srcEnumDef)
            {
                // Clone options (resolve parent option via optMap)
                foreach (var srcOpt in srcEnumDef.Options.OrderBy(o => o.Position))
                {
                    EnumAttributeOption? dstParentOpt = null;
                    if (srcOpt.ParentOption is { } srcParentOpt)
                    {
                        dstParentOpt = optMap[(srcParentOpt.EnumAttributeDefinition.Key, srcParentOpt.Code)];
                    }

                    var dstOpt = target.AddAttributeOption(
                        key: dstDef.Key,
                        optionCode: srcOpt.Code,
                        position: srcOpt.Position,
                        parentOptionCode: dstParentOpt?.Code);

                    optMap[(srcDef.Key, srcOpt.Code)] = dstOpt;
                    CopyOptionTranslations(srcOpt, dstOpt);
                }
            }
            else if (srcDef is LookupAttributeDefinition srcLookupDef)
            {
                // Clone allowed lookup options
                foreach (var srcAllowedOpt in srcLookupDef.AllowedOptions)
                {
                    target.AddAllowedLookup(srcDef.Key, srcAllowedOpt.Option);
                }
            }
        }
    }

    private static void ExtractAttrDefConfig(
        AttributeDefinition def,
        out string? unit,
        out decimal? min,
        out decimal? max,
        out decimal? step,
        out string? regex,
        out LookupType? lookupType)
    {
        min = max = step = null;
        unit = regex = null;
        lookupType = null;

        switch (def)
        {
            case NumericAttributeDefinition<int> i:
                unit = i.Config.Unit;
                min = i.Config.Min;
                max = i.Config.Max;
                step = i.Config.Step;
                break;
            case NumericAttributeDefinition<decimal> d:
                unit = d.Config.Unit;
                min = d.Config.Min;
                max = d.Config.Max;
                step = d.Config.Step;
                break;
            case StringAttributeDefinition s:
                regex = s.Config.Regex;
                break;
            case EnumAttributeDefinition:
                break;
            case LookupAttributeDefinition l:
                lookupType = l.LookupType;
                break;
            default:
                break;
        }
    }

    private static void CopyDefinitionTranslations(AttributeDefinition src, AttributeDefinition dst)
    {
        foreach (var t in src.Translations)
        {
            dst.Translations.Add(new Domain.Translations.AttributeDefinitionTr
            {
                LangCode = t.LangCode,
                Name = t.Name,
                Unit = t.Unit,
            });
        }
    }

    private static void CopyOptionTranslations(EnumAttributeOption src, EnumAttributeOption dst)
    {
        foreach (var t in src.Translations)
        {
            dst.Translations.Add(new Domain.Translations.EnumAttributeOptionTr
            {
                LangCode = t.LangCode,
                Name = t.Name,
            });
        }
    }
}
