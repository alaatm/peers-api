using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Lookup.Domain;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Represents a value assigned to a specific attribute of a listing, including its type, value, and related metadata.
/// </summary>
/// <remarks>A ListingAttribute encapsulates the value and metadata for a single attribute instance on a listing,
/// supporting various attribute kinds such as numeric, string, boolean, date, enum, and lookup types. The actual value
/// may be stored as a string or as a reference to an enum or lookup option, depending on the attribute kind.</remarks>
public sealed class ListingAttribute
{
    /// <summary>
    /// The identifier of the listing this attribute belongs to.
    /// </summary>
    public int ListingId { get; private set; }
    /// <summary>
    /// The identifier of the attribute definition this attribute is based on.
    /// </summary>
    public int AttributeDefinitionId { get; private set; }
    /// <summary>
    /// The identifier of the selected enum option, if this attribute is of enum type.
    /// </summary>
    public int? EnumAttributeOptionId { get; private set; }
    /// <summary>
    /// The identifier of the selected lookup value, if this attribute is of lookup type.
    /// </summary>
    public int? LookupValueId { get; private set; }
    /// <summary>
    /// The kind of attribute (e.g., int, decimal, string, bool, date, enum, lookup).
    /// </summary>
    public AttributeKind AttributeKind { get; private set; }
    /// <summary>
    /// The value of the attribute as a string. For enum and lookup types, this is null and the selected option/value is stored
    /// in the respective navigation properties.
    /// </summary>
    public string? Value { get; private set; } = default!;
    /// <summary>
    /// The position of the attribute in the product type's attribute list, used for ordering.
    /// </summary>
    public int Position { get; private set; }
    /// <summary>
    /// The listing this attribute belongs to.
    /// </summary>
    public Listing Listing { get; private set; } = default!;
    /// <summary>
    /// The attribute definition this attribute is based on.
    /// </summary>
    public AttributeDefinition AttributeDefinition { get; internal set; } = default!;
    /// <summary>
    /// The selected enum option if this attribute is of enum type; otherwise, null.
    /// </summary>
    public EnumAttributeOption? EnumAttributeOption { get; internal set; }
    /// <summary>
    /// The selected lookup value if this attribute is of lookup type; otherwise, null.
    /// </summary>
    public LookupValue? LookupValue { get; private set; }

    private ListingAttribute() { }

    private ListingAttribute(
        Listing listing,
        AttributeDefinition def,
        EnumAttributeOption? option = null,
        LookupValue? lookupValue = null,
        string? value = null)
    {
        Listing = listing;
        AttributeDefinition = def;
        EnumAttributeOption = option;
        LookupValue = lookupValue;
        AttributeKind = def.Kind;
        Value = value;
        Position = def.Position;
    }

    /// <summary>
    /// Creates a new ListingAttribute instance for the specified listing and attribute definition using the provided
    /// value.
    /// </summary>
    /// <param name="listing">The listing to which the attribute will be associated.</param>
    /// <param name="def">The attribute definition that determines the type and constraints of the attribute to create.</param>
    /// <param name="value">The value to assign to the attribute. The format and constraints depend on the type of the attribute definition.</param>
    internal static ListingAttribute Create(Listing listing, AttributeDefinition def, string value) => def switch
    {
        IntAttributeDefinition i => OfNumeric(listing, i, value),
        DecimalAttributeDefinition d => OfNumeric(listing, d, value),
        StringAttributeDefinition s => OfString(listing, s, value),
        BoolAttributeDefinition b => OfBool(listing, b, value),
        DateAttributeDefinition dt => OfDate(listing, dt, value),
        EnumAttributeDefinition e => OfEnum(listing, e, value),
        LookupAttributeDefinition l => OfLookup(listing, l, value),
        _ => throw new UnreachableException(),
    };

    private static ListingAttribute OfNumeric<T>(Listing listing, NumericAttributeDefinition<T> def, string value)
        where T : struct, INumber<T>
    {
        if (!T.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var iv))
        {
            throw new DomainException(E.AttrValueMustBeNumeric<T>(def.Key, value));
        }

        ValidateNumericRange(def.Key, iv, def.Config.Min, def.Config.Max);
        return new(listing, def, value: iv.ToString(null, CultureInfo.InvariantCulture));
    }

    private static ListingAttribute OfString(Listing listing, StringAttributeDefinition def, string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            (def.Config.Regex is string r && !Regex.IsMatch(value, r)))
        {
            throw new DomainException(E.AttrValueMustBeValidString(def.Key, value, def.Config.Regex));
        }

        return new(listing, def, value: value);
    }

    private static ListingAttribute OfBool(Listing listing, BoolAttributeDefinition def, string value)
    {
        if (!bool.TryParse(value, out var bv))
        {
            throw new DomainException(E.AttrValueMustBeBool(def.Key, value));
        }

        return new(listing, def, value: bv ? "true" : "false");
    }

    private static ListingAttribute OfDate(Listing listing, DateAttributeDefinition def, string value)
    {
        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            throw new DomainException(E.AttrValueMustBeDate(def.Key, value));
        }

        return new(listing, def, value: value);
    }

    private static ListingAttribute OfEnum(Listing listing, EnumAttributeDefinition def, string value)
    {
        if (def.Options.FirstOrDefault(p => p.Key == value) is not EnumAttributeOption option)
        {
            throw new DomainException(E.UnknownAttrOption(def.Key, value));
        }

        return new(listing, def, option: option);
    }

    private static ListingAttribute OfLookup(Listing listing, LookupAttributeDefinition def, string value)
    {
        if (def.LookupType.Values.FirstOrDefault(p => p.Key == value) is not LookupValue lookupValue)
        {
            throw new DomainException(E.UnknownAttrOption(def.Key, value));
        }

        if (!listing.ProductType.IsLookupValueAllowed(lookupValue, noEntriesMeansAllowAll: true))
        {
            throw new DomainException(E.LookupValueNotAllowedForProductType(value, def.Key, listing.ProductType.SlugPath));
        }

        return new(listing, def, lookupValue: lookupValue);
    }

    private static void ValidateNumericRange<T>(string key, T value, T? min, T? max) where T : struct, INumber<T>
    {
        if (min.HasValue && value.CompareTo(min.Value) < 0)
        {
            throw new DomainException(E.AttrValueMustBeAtLeast(key, value, min.Value));
        }
        if (max.HasValue && value.CompareTo(max.Value) > 0)
        {
            throw new DomainException(E.AttrValueMustBeAtMost(key, value, max.Value));
        }
    }
}
