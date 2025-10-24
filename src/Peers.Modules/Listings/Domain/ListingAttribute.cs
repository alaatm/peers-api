using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Peers.Core.Domain.Errors;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Lookup.Domain;
using static Peers.Modules.Listings.Commands.SetAttributes.Command;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Represents a value assigned to a specific attribute of a listing, including its type, value, and related metadata.
/// </summary>
/// <remarks>A ListingAttribute encapsulates the value and metadata for a single attribute instance on a listing,
/// supporting various attribute kinds such as numeric, string, boolean, date, enum, and lookup types. The actual value
/// may be stored as a string or as a reference to an enum or lookup option, depending on the attribute kind.</remarks>
[DebuggerDisplay("{D,nq}")]
public sealed partial class ListingAttribute : IDebuggable
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
    /// The identifier of the selected lookup option, if this attribute is of lookup type.
    /// </summary>
    public int? LookupOptionId { get; private set; }
    /// <summary>
    /// The kind of attribute (e.g., int, decimal, string, bool, date, enum, lookup).
    /// </summary>
    public AttributeKind AttributeKind { get; private set; }
    /// <summary>
    /// The value of the attribute as a string. For enum and lookup types, this is null and the selected option is stored
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
    /// The selected lookup option if this attribute is of lookup type; otherwise, null.
    /// </summary>
    public LookupOption? LookupOption { get; private set; }

    private ListingAttribute() { }

    private ListingAttribute(
        Listing listing,
        AttributeDefinition def,
        EnumAttributeOption? option = null,
        LookupOption? lookupOption = null,
        string? value = null)
    {
        Listing = listing;
        AttributeDefinition = def;
        EnumAttributeOption = option;
        LookupOption = lookupOption;
        AttributeKind = def.Kind;
        Value = value;
        Position = def.Position;
    }

    /// <summary>
    /// Creates a new ListingAttribute instance for the specified listing using the provided attribute definition and input value.
    /// </summary>
    /// <param name="listing">The listing to which the attribute will be associated.</param>
    /// <param name="def">The attribute definition that determines the type and constraints of the attribute to create.</param>
    /// <param name="input">The input value to assign to the attribute. The format and constraints depend on the type of the attribute definition.</param>
    internal static ListingAttribute Create(Listing listing, AttributeDefinition def, AttributeInputDto input) => def switch
    {
        IntAttributeDefinition i => OfNumeric(listing, i, input),
        DecimalAttributeDefinition d => OfNumeric(listing, d, input),
        StringAttributeDefinition s => OfString(listing, s, input),
        BoolAttributeDefinition b => OfBool(listing, b, input),
        DateAttributeDefinition dt => OfDate(listing, dt, input),
        EnumAttributeDefinition e => OfEnum(listing, e, input),
        LookupAttributeDefinition l => OfLookup(listing, l, input),
        _ => throw new UnreachableException(),
    };

    private static ListingAttribute OfNumeric<T>(Listing listing, NumericAttributeDefinition<T> def, AttributeInputDto input)
        where T : struct, INumber<T>
    {
        if (input is AttributeInputDto.Numeric(var value))
        {
            def.ValidateValue(value);
            return new(listing, def, value: value.ToString(CultureInfo.InvariantCulture));
        }

        throw new DomainException(E.AttrReqSingleNumValue(def.Key));
    }

    private static ListingAttribute OfString(Listing listing, StringAttributeDefinition def, AttributeInputDto input)
    {
        if (input is AttributeInputDto.OptionCodeOrScalarString(var value))
        {
            def.ValidateValue(value);
            return new(listing, def, value: value);
        }

        throw new DomainException(E.AttrReqSingleStrValue(def.Key));
    }

    private static ListingAttribute OfBool(Listing listing, BoolAttributeDefinition def, AttributeInputDto input)
    {
        if (input is AttributeInputDto.Bool(var value))
        {
            return new(listing, def, value: value ? "true" : "false");
        }

        throw new DomainException(E.AttrReqSingleBoolValue(def.Key));
    }

    private static ListingAttribute OfDate(Listing listing, DateAttributeDefinition def, AttributeInputDto input)
    {
        if (input is AttributeInputDto.Date(var value))
        {
            return new(listing, def, value: value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        throw new DomainException(E.AttrReqSingleDateValue(def.Key));
    }

    private static ListingAttribute OfEnum(Listing listing, EnumAttributeDefinition def, AttributeInputDto input)
    {
        if (input is AttributeInputDto.OptionCodeOrScalarString(var value))
        {
            if (def.Options.FirstOrDefault(p => p.Code == value) is not EnumAttributeOption option)
            {
                throw new DomainException(E.UnknownEnumAttrOpt(def.Key, value));
            }

            return new(listing, def, option: option);
        }

        throw new DomainException(E.AttrReqSingleEnumOptCodeValue(def.Key));
    }

    private static ListingAttribute OfLookup(Listing listing, LookupAttributeDefinition def, AttributeInputDto input)
    {
        if (input is AttributeInputDto.OptionCodeOrScalarString(var value))
        {
            if (def.LookupType.Options.FirstOrDefault(p => p.Code == value) is not LookupOption option)
            {
                throw new DomainException(E.UnknownLookupAttrOpt(def.Key, value));
            }

            if (!def.IsOptionAllowed(option, noEntriesMeansAllowAll: true))
            {
                throw new DomainException(E.LookupOptNotAllowedByAttr(def.Key, value));
            }

            return new(listing, def, lookupOption: option);
        }

        throw new DomainException(E.AttrReqSingleLookupOptCodeValue(def.Key));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"LA:{ListingId}:{AttributeDefinitionId} - {AttributeDefinition.D} = {Value ?? EnumAttributeOption?.D ?? LookupOption?.D ?? "<null>"}";
}
