using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Domain;

public static class ProductTypeExtensions
{
    extension(ProductType pt)
    {
        public IntAttributeDefinition DefineIntAttribute(
            string key,
            bool isRequired,
            int position,
            string? unit = null,
            int? min = null,
            int? max = null)
            => (IntAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Int, isRequired, position, unit: unit, minInt: min, maxInt: max);

        public DecimalAttributeDefinition DefineDecimalAttribute(
            string key,
            bool isRequired,
            int position,
            string? unit = null,
            decimal? min = null,
            decimal? max = null)
            => (DecimalAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Decimal, isRequired, position, unit: unit, minDecimal: min, maxDecimal: max);

        public StringAttributeDefinition DefineStringAttribute(
            string key,
            bool isRequired,
            int position,
            string? regex = null)
            => (StringAttributeDefinition)pt.DefineAttribute(key, AttributeKind.String, isRequired, position, regex: regex);

        public BoolAttributeDefinition DefineBoolAttribute(
            string key,
            bool isRequired,
            int position)
            => (BoolAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Bool, isRequired, position);

        public DateAttributeDefinition DefineDateAttribute(
            string key,
            bool isRequired,
            int position)
            => (DateAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Date, isRequired, position);

        public EnumAttributeDefinition DefineEnumAttribute(
            string key,
            bool isRequired,
            int position,
            bool isVariant)
            => (EnumAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Enum, isRequired, position, isVariant: isVariant);

        public LookupAttributeDefinition DefineLookupAttribute(
            string key,
            bool isRequired,
            int position,
            LookupType lookupType)
            => (LookupAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Lookup, isRequired, position, lookupType: lookupType);

        public EnumAttributeDefinition DefineDependentEnumAttribute(
            string parentKey,
            string key,
            bool isRequired,
            int position,
            bool isVariant)
            => (EnumAttributeDefinition)pt.DefineDependentAttribute(parentKey, key, AttributeKind.Enum, isRequired, position, isVariant: isVariant);

        public LookupAttributeDefinition DefineDependentLookupAttribute(
            string parentKey,
            string key,
            bool isRequired,
            int position,
            LookupType lookupType)
            => (LookupAttributeDefinition)pt.DefineDependentAttribute(parentKey, key, AttributeKind.Lookup, isRequired, position, lookupType: lookupType);
    }
}
