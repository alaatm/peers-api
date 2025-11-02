using System.Diagnostics;
using Peers.Core.Domain.Errors;
using E = Peers.Modules.Lookup.LookupErrors;

namespace Peers.Modules.Lookup.Domain;

/// <summary>
/// Logical catalog for a family of lookup types (e.g., "brand", "device_model").
/// </summary>
/// <remarks>
/// - Keys are global and stable; one <see cref="LookupType"/> is reused across many product types.
/// - Referenced by <see cref="Catalog.Domain.Attributes.LookupAttributeDefinition"/>.
/// - Uniqueness: <c>Key</c> is unique; values under this type enforce their own (TypeId, Key) uniqueness.
/// </remarks>
[DebuggerDisplay("{D,nq}")]
public sealed class LookupType : Entity, IAggregateRoot
{
    /// <summary>
    /// Stable ASCII key/slug for this type (e.g., "brand", "model").
    /// </summary>
    public string Key { get; set; } = default!;
    /// <summary>
    /// The default constraint behavior applied when this lookup type is used
    /// by a <see cref="Catalog.Domain.Attributes.LookupAttributeDefinition"/> and no explicit override is set.
    /// </summary>
    /// <remarks>
    /// - If <see cref="LookupConstraintMode.Open"/>, listings may use any option of this lookup type
    ///   unless a product type (or its nearest ancestor) declares an allow-list.
    /// - If <see cref="LookupConstraintMode.RequireAllowList"/>, a product type must provide an
    ///   allow-list in its lineage for options of this type to be considered valid.
    /// </remarks>
    public LookupConstraintMode ConstraintMode { get; set; }
    /// <summary>
    /// Indicates whether variant-level attributes of this type are allowed.
    /// </summary>
    public bool AllowVariant { get; set; }
    /// <summary>
    /// The list of lookup options associated with this type.
    /// </summary>
    public List<LookupOption> Options { get; set; } = default!;
    /// <summary>
    /// The list of links where this type is the parent.
    /// </summary>
    public List<LookupLink> ParentLinks { get; private set; } = default!;
    /// <summary>
    /// The list of links where this type is the child.
    /// </summary>
    public List<LookupLink> ChildLinks { get; private set; } = default!;

    private LookupType()
    {
    }

    /// <summary>
    /// Initializes a new instance of the LookupType class with the specified key, constraint mode, and variant
    /// allowance.
    /// </summary>
    /// <param name="key">The unique identifier for the lookup type. Must be in snake_case format.</param>
    /// <param name="constraintMode">The constraint mode that determines how lookups are validated or restricted.</param>
    /// <param name="allowVariant">A value indicating whether variants are permitted for this lookup type.</param>
    public LookupType(
        string key,
        LookupConstraintMode constraintMode,
        bool allowVariant)
    {
        if (!RegexStatic.IsSnakeCaseRegex().IsMatch(key))
        {
            throw new DomainException(E.KeyFormatInvalid(key));
        }

        Key = key;
        ConstraintMode = constraintMode;
        AllowVariant = allowVariant;
        Options = [];
        ParentLinks = [];
        ChildLinks = [];
    }

    /// <summary>
    /// Creates a new option with the specified code and adds it to the collection of options.
    /// </summary>
    /// <param name="code">The code that uniquely identifies the option to create. Must be in snake_case format.</param>
    public void CreateOption(string code)
    {
        if (!RegexStatic.IsSnakeCaseRegex().IsMatch(code))
        {
            throw new DomainException(E.KeyFormatInvalid(code));
        }

        var option = new LookupOption(code, this);
        Options.Add(option);
    }

    /// <summary>
    /// Establishes links between a parent option and one or more child options of the specified type.
    /// </summary>
    /// <param name="parentOptCode">The code identifying the parent option to which child options will be linked.</param>
    /// <param name="childType">The lookup type representing the category of child options to link.</param>
    /// <param name="childOptCodes">An array of codes identifying the child options to link to the parent option. Each code must correspond to an
    /// existing option in the specified child type.</param>
    public void LinkOptions(
        [NotNull] string parentOptCode,
        [NotNull] LookupType childType,
        [NotNull] string[] childOptCodes)
    {
        var parentOpt = Options.Find(o => o.Code == parentOptCode)
            ?? throw new DomainException(E.ParentOptNotFound(parentOptCode, Key));

        var childOptByCode = childType.Options.ToDictionary(o => o.Code, StringComparer.Ordinal);

        // Current links for this (parentType -> childType) pair
        var existing = ParentLinks
            .Where(p => p.ChildType == childType && p.ParentOption == parentOpt)
            .Select(p => p.ChildOption.Code)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var childOptCode in childOptCodes)
        {
            if (!childOptByCode.TryGetValue(childOptCode, out var childOpt))
            {
                throw new DomainException(E.ChildOptNotFound(childOptCode, childType.Key));
            }

            if (existing.Contains(childOptCode))
            {
                continue;
            }

            var link = new LookupLink
            {
                ParentType = this,
                ParentOption = parentOpt,
                ChildType = childType,
                ChildOption = childOpt
            };

            ParentLinks.Add(link);
            childType.ChildLinks.Add(link);
            existing.Add(childOptCode);
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"LT:{Id} - {Key} ({ConstraintMode})";
}
