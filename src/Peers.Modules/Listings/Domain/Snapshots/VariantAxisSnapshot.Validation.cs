using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Validation;

namespace Peers.Modules.Listings.Domain.Snapshots;


public partial record VariantAxisSnapshot
{
    /// <summary>
    /// Validates the following:
    /// 1. All attribute definitions exist in the product type schema.
    /// 2. All attribute definitions are unique.
    /// 2. All attribute definitions are variant.
    /// 3. For non-group definitions, they must be either enum, lookup or numeric.
    /// 4. For group definitions:
    ///     a. They must be of GroupAttributeDefinition.
    ///     b. All group members are unique.
    /// </summary>
    /// <param name="ctx">The validation context.</param>
    /// <param name="schemaAxesDefSet">Populated with attribute definitions and group member definitions upon return from this axis snapshot.</param>
    internal void ValidateSchema(
        ValidationContext ctx,
        HashSet<AttributeDefinition> schemaAxesDefSet)
    {
        if (ctx.DefByKey.TryGetValue(DefinitionKey, out var def))
        {
            if (!def.IsVariant)
            {
                throw StateError($"Axis definition '{def}' is not a variant attribute.");
            }

            if (!IsGroup)
            {
                if (def is not (EnumAttributeDefinition or LookupAttributeDefinition or NumericAttributeDefinition))
                {
                    throw StateError($"Axis definition '{def}' expected to be enum/lookup or numeric but is {def.GetType().Name}.");
                }
                if (!schemaAxesDefSet.Add(def))
                {
                    throw StateError($"Duplicate axis definition '{def}' in axes snapshot.");
                }
            }
            else
            {
                if (def is GroupAttributeDefinition groupDef)
                {
                    foreach (var member in groupDef.Members)
                    {
                        if (!schemaAxesDefSet.Add(member))
                        {
                            throw StateError($"Duplicate axis definition '{member}' in axes snapshot (via group '{def}').");
                        }
                    }
                }
                else
                {
                    throw StateError($"Axis definition '{def}' expected to be a group but is {def.GetType().Name}.");
                }
            }
        }
        else
        {
            throw StateError($"Axis definition key '{DefinitionKey}' not found in '{ctx.ProductType}' product type schema.");
        }
    }

    /// <summary>
    /// Must call after <see cref="ValidateSchema"/>.
    /// Validates the following:
    /// 1. For non-group definitions, the variant must have an attribute matching one of the axis choices.
    /// 2. For group definitions, the variant must have attributes for all group members matching one of the axis choices.
    /// 3. The variant must not have any attributes beyond those defined in the schema axes (and group members, if applicable).
    /// </summary>
    /// <param name="ctx">The validation context.</param>
    /// <param name="variant">The variant to check coverage against this axis.</param>
    /// <param name="variantAttrsByDef">The variant's attributes indexed by definition.</param>
    /// <param name="schemaAxesDefSet">The set populated by <see cref="ValidateSchema"/>.</param>
    internal void ValidateVariantCoverage(
        ValidationContext ctx,
        ListingVariant variant,
        Dictionary<AttributeDefinition, ListingVariantAttribute> variantAttrsByDef,
        HashSet<AttributeDefinition> schemaAxesDefSet)
    {
        var def = ctx.DefByKey[DefinitionKey];

        if (!IsGroup)
        {
            if (!variantAttrsByDef.TryGetValue(def, out var variantAttr))
            {
                throw StateError($"Attribute definition '{def}' not found in variant '{variant.D}' attributes.");
            }

            // Ensure this attribute's value matches one of the axis choices
            var match = Choices.Any(c =>
                (c.EnumOptionCode is not null && variantAttr.EnumAttributeOption?.Code == c.EnumOptionCode) ||
                (c.LookupOptionCode is not null && variantAttr.LookupOption?.Code == c.LookupOptionCode) ||
                (c.NumericValue is not null && variantAttr.NumericValue == c.NumericValue));

            if (!match)
            {
                throw StateError($"Attribute value '{variantAttr.D}' not found in choices");
            }
        }
        else
        {
            var groupDef = (GroupAttributeDefinition)def;

            var orderedMembers = groupDef.Members
                .OrderBy(m => m.Position).ThenBy(m => m.Key, StringComparer.Ordinal)
                .ToArray();

            // Collect this variant's member values in the same order
            var memberValues = new List<decimal>(orderedMembers.Length);
            foreach (var m in orderedMembers)
            {
                if (!variantAttrsByDef.TryGetValue(m, out var attr))
                {
                    throw StateError($"Group member attribute definition '{m.D}' not found in variant '{variant.D}' attributes.");
                }
                if (attr.NumericValue is not null)
                {
                    memberValues.Add(attr.NumericValue.Value);
                }
            }

            // Must match exactly one group choice in snapshot
            var exists = Choices.Any(c =>
                c.GroupMembers is not null &&
                c.GroupMembers.Count == memberValues.Count &&
                c.GroupMembers.Select(x => x.Value).SequenceEqual(memberValues));

            if (!exists)
            {
                throw StateError($"Group member values '{string.Join(',', memberValues)}' not found in choices.");
            }
        }


        // No extra attributes beyond the axis definition (and group members, if applicable) are allowed.
        foreach (var attrDef in variantAttrsByDef.Keys)
        {
            if (!schemaAxesDefSet.Contains(attrDef))
            {
                throw StateError($"Variant '{variant.VariantKey}' has an attribute '{attrDef}' that is not part of the variant axes schema.");
            }
        }
    }

    private InvalidDomainStateException StateError(string message, Exception? inner = null) => inner is null
        ? throw new InvalidDomainStateException(this, message)
        : throw new InvalidDomainStateException(this, message, inner);
}
