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
            bool isVariant,
            int position,
            string? unit = null,
            int? min = null,
            int? max = null,
            int? step = null)
            => (IntAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Int, isRequired, isVariant, position, unit: unit, min: min, max: max, step: step);

        public DecimalAttributeDefinition DefineDecimalAttribute(
            string key,
            bool isRequired,
            bool isVariant,
            int position,
            string? unit = null,
            decimal? min = null,
            decimal? max = null,
            decimal? step = null)
            => (DecimalAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Decimal, isRequired, isVariant, position, unit: unit, min: min, max: max, step: step);

        public StringAttributeDefinition DefineStringAttribute(
            string key,
            bool isRequired,
            int position,
            string? regex = null)
            => (StringAttributeDefinition)pt.DefineAttribute(key, AttributeKind.String, isRequired, false, position, regex: regex);

        public BoolAttributeDefinition DefineBoolAttribute(
            string key,
            bool isRequired,
            int position)
            => (BoolAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Bool, isRequired, false, position);

        public DateAttributeDefinition DefineDateAttribute(
            string key,
            bool isRequired,
            int position)
            => (DateAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Date, isRequired, false, position);

        public EnumAttributeDefinition DefineEnumAttribute(
            string key,
            bool isRequired,
            bool isVariant,
            int position)
            => (EnumAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Enum, isRequired, isVariant, position);

        public LookupAttributeDefinition DefineLookupAttribute(
            string key,
            bool isRequired,
            bool isVariant,
            int position,
            LookupType lookupType)
            => (LookupAttributeDefinition)pt.DefineAttribute(key, AttributeKind.Lookup, isRequired, isVariant, position, lookupType: lookupType);
    }
}
