using System.Globalization;
using Peers.Core.Domain.Errors;
using Peers.Modules.Lookup.Domain;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents the definition of an attribute whose value is selected from a global and shared predefined lookup type.
/// </summary>
/// <remarks>A lookup attribute definition specifies that the attribute's value must correspond to an entry in the
/// associated lookup type. This class is typically used to enforce consistency and restrict attribute values to a
/// controlled set defined by the lookup type.</remarks>
public sealed class LookupAttributeDefinition : AttributeDefinition
{
    /// <summary>
    /// The identifier of the associated lookup type.
    /// </summary>
    public int LookupTypeId { get; private set; }
    public LookupAttrConfig Config { get; set; }
    /// <summary>
    /// The associated lookup type.
    /// </summary>
    public LookupType LookupType { get; private set; } = default!;
    /// <summary>
    /// The list of allowed lookup options for this lookup attribute.
    /// </summary>
    /// <remarks>
    /// Rows exist → only listed options are allowed.
    /// No rows    → treat as "allow all" but can be overridden by policy.
    /// </remarks>
    public List<LookupAllowed> AllowedOptions { get; private set; } = default!;

    private LookupAttributeDefinition() : base() { }

    internal LookupAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        bool isVariant,
        int position,
        LookupType? lookupType) : base(owner, key, AttributeKind.Lookup, isRequired, isVariant, position)
    {
        ArgumentNullException.ThrowIfNull(lookupType);

        if (!lookupType.AllowVariant && isVariant)
        {
            throw new DomainException(E.LookupTypeDoesNotAllowVariants(key, lookupType.Key));
        }

        LookupType = lookupType;
        // TODO: Allow override
        Config = new LookupAttrConfig { ConstraintMode = lookupType.ConstraintMode };
        AllowedOptions = [];
    }

    /// <summary>
    /// Returns true if the speicifed lookup option is permitted by this lookup attribute. If no allow-list is declared, the fallback policy
    /// applies as set in noEntriesMeansAllowAll.
    /// </summary>
    /// <param name="option">The lookup option to check.</param>
    /// <param name="noEntriesMeansAllowAll">If true, the absence of allow-list entries means all options of the lookup type are allowed; if false, none are allowed.</param>
    /// <returns></returns>
    public bool IsOptionAllowed(
        [NotNull] LookupOption option,
        bool noEntriesMeansAllowAll)
    {
        if (option.Type != LookupType)
        {
            throw new DomainException(E.LookupOptionDoesNotBelongToType(option.Code, LookupType.Key));
        }

        if (AllowedOptions.Count > 0 &&
            AllowedOptions.Any(p => p.Option.Code == option.Code))
        {
            return true;
        }

        // No allow-list declaration, fallback to policy
        return noEntriesMeansAllowAll;
    }

    internal void AddAllowedOption(LookupOption option)
    {
        if (option.Type != LookupType)
        {
            throw new DomainException(E.LookupOptionDoesNotBelongToType(option.Code, LookupType.Key));
        }

        if (AllowedOptions.Any(p => p.Option.Code == option.Code))
        {
            throw new DomainException(E.DuplicateLookupAllowedOption(option.Code, Key));
        }

        var allowed = new LookupAllowed(this, option);
        AllowedOptions.Add(allowed);
    }

    internal void RemoveAllowedOption(LookupOption option)
    {
        if (AllowedOptions.FirstOrDefault(p => p.Option.Code == option.Code) is not { } allowedOpt)
        {
            throw new DomainException(E.LookupAllowedOptionNotFound(option.Code, Key));
        }

        AllowedOptions.Remove(allowedOpt);
    }

    internal override void Validate()
    {
        base.Validate();

        if (!LookupType.AllowVariant && IsVariant)
        {
            throw new DomainException(E.LookupTypeDoesNotAllowVariants(Key, LookupType.Key));
        }

        if (LookupType.ConstraintMode is LookupConstraintMode.RequireAllowList &&
            AllowedOptions.Count == 0)
        {
            throw new DomainException(E.MissingLookupAllowList(Key, LookupType.Key));
        }
    }

    public override string D
        => $"{base.D} | LookupType: {LookupType?.Key ?? LookupTypeId.ToString(CultureInfo.InvariantCulture)}";
}
