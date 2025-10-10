using System.Diagnostics;
using Peers.Core.Domain.Errors;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.Catalog.Utils;
using Peers.Modules.Lookup.Domain;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Domain;

/// <summary>
/// Represents a hierarchical product type with a specific kind, state, and versioning capabilities.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class ProductType : Entity, IAggregateRoot, ILocalizable<ProductType, ProductTypeTr>
{
    /// <summary>
    /// The kind of the product type.
    /// </summary>
    public ProductTypeKind Kind { get; private set; }
    /// <summary>
    /// The state of the product type.
    /// </summary>
    public ProductTypeState State { get; private set; }
    /// <summary>
    /// The slug of the product type. Unique under the same parent.
    /// </summary>
    public string Slug { get; private set; } = default!;
    /// <summary>
    /// The full slug path of the product type, including parent slugs separated by '/'.
    /// </summary>
    public string SlugPath { get; private set; } = default!;
    /// <summary>
    /// Leaf marker; indicates if sellers can create listings from this product type.
    /// </summary>
    public bool IsSelectable { get; private set; }
    /// <summary>
    /// The product type version.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// The identifier of the parent product type, if any.
    /// </summary>
    public int? ParentId { get; private set; }
    /// <summary>
    /// The parent product type, if any.
    /// </summary>
    public ProductType? Parent { get; private set; }

    /// <summary>
    /// The list of child product types.
    /// </summary>
    public List<ProductType> Children { get; private set; } = default!;
    /// <summary>
    /// The list of attribute definitions associated with this product type.
    /// </summary>
    public List<AttributeDefinition> Attributes { get; private set; } = default!;
    /// <summary>
    /// The list of allowed lookup options for lookup attributes in this product type.
    /// </summary>
    public List<LookupAllowed> LookupsAllowed { get; private set; } = default!;
    /// <summary>
    /// The list of translations associated with this product type.
    /// </summary>
    public List<ProductTypeTr> Translations { get; private set; } = default!;


    private ProductType() { }

    private ProductType(ProductTypeKind kind, string slug, string slugPath, bool isSelectable, int version)
    {
        Kind = kind;
        State = ProductTypeState.Draft;
        Slug = slug;
        SlugPath = slugPath;
        IsSelectable = isSelectable;
        Version = version;
        Children = [];
        Attributes = [];
        LookupsAllowed = [];
        Translations = [];
    }

    /// <summary>
    /// Instantiate a new root product type as version 1 and in Draft state.
    /// </summary>
    /// <param name="kind">The kind of the product type.</param>
    /// <param name="name">The name of the product type.</param>
    /// <returns></returns>
    public static ProductType CreateRoot(
        ProductTypeKind kind,
        string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var slug = SlugHelper.ToSlug(name);
        return new(kind, slug, $"/{slug}", false, 1);
    }

    /// <summary>
    /// Creates and adds a new child product type with the specified name and options.
    /// </summary>
    /// <param name="name">The name of the child product type to which is used to generate the slug.</param>
    /// <param name="isSelectable">Indicates whether the new child product type can be used for creating listings by sellers.</param>
    /// <param name="copyAttributes">Indicates whether to copy the attribute schema from the current product type to the new child.</param>
    /// <param name="version">The version number to assign to the new child product type. If null, the current product type's version is used.</param>
    public ProductType AddChild(
        string name,
        bool isSelectable,
        bool copyAttributes,
        int? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (State is not ProductTypeState.Published)
        {
            throw new DomainException(E.NotPublished);
        }

        var childSlug = SlugHelper.ToSlug(name);

        if (Children.Any(p => p.Slug == childSlug))
        {
            throw new DomainException(E.ChildAlreadyExists(name.Trim()));
        }

        var child = new ProductType(Kind, childSlug, $"{SlugPath}/{childSlug}", isSelectable, version ?? Version);
        Children.Add(child);

        if (copyAttributes)
        {
            AttributeSchemaCloner.CopyFrom(this, child);
        }

        return child;
    }

    /// <summary>
    /// Creates a new ProductType instance representing the next version of the current product type.
    /// </summary>
    /// <remarks>The new version will have the same slug and selectability as the current product type.
    /// Translations are always copied. The method must be called on a published product type.</remarks>
    /// <param name="copyAttributes">true to copy attribute definitions and lookup permissions from the current version; false to exclude them from
    /// the new version. When true, also copies LookupAllowed entries.</param>
    public ProductType CloneAsNextVersion(bool copyAttributes)
    {
        // Create the child under the same parent without copying parent attributes as we want to copy from 'this' (sibling)
        var next = Parent!.AddChild(Slug, IsSelectable, copyAttributes: false, Version + 1);

        // Copy translations
        foreach (var tr in Translations)
        {
            next.Translations.Add(new ProductTypeTr
            {
                LangCode = tr.LangCode,
                Name = tr.Name,
            });
        }

        if (copyAttributes)
        {
            AttributeSchemaCloner.CopyFrom(this, next);

            // Copy allowed lookup entries only if attributes were copied
            foreach (var la in LookupsAllowed)
            {
                next.LookupsAllowed.Add(new LookupAllowed(next, la.Option));
            }
        }

        return next;
    }

    /// <summary>
    /// Defines a new attribute for the product type with the specified properties.
    /// </summary>
    /// <param name="key">The unique key for the attribute.</param>
    /// <param name="kind">The kind of the attribute.</param>
    /// <param name="isRequired">Indicates whether the attribute is required.</param>
    /// <param name="position">The position of the attribute in the attribute list.</param>
    /// <param name="isVariant">Indicates whether this attribute's value creates a unique, sellable variant of a listing.</param>
    /// <param name="lookupType">The lookup type associated with the lookup attribute if applicable.</param>
    /// <param name="unit">The unit of measurement for the attribute if applicable.</param>
    /// <param name="min">The minimum value for numeric attributes if applicable.</param>
    /// <param name="max">The maximum value for numeric attributes if applicable.</param>
    /// <param name="step">The step/increment value for numeric attributes if applicable.</param>
    /// <param name="regex">A regular expression pattern for string attributes if applicable.</param>
    public AttributeDefinition DefineAttribute(
        string key,
        AttributeKind kind,
        bool isRequired,
        bool isVariant,
        int position,
        LookupType? lookupType = null,
        string? unit = null,
        decimal? min = null,
        decimal? max = null,
        decimal? step = null,
        string? regex = null)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (!RegexStatic.IsSnakeCaseRegex().IsMatch(key))
        {
            throw new DomainException(E.KeyFormatInvalid(key));
        }

        if (Attributes.Any(p => p.Key == key))
        {
            throw new DomainException(E.AttrAlreadyExists(key));
        }

        if (isVariant &&
            kind is AttributeKind.Bool or AttributeKind.Date or AttributeKind.String)
        {
            throw new DomainException(E.VariantNotAllowedForBoolStrDate);
        }

        if (kind is AttributeKind.Group)
        {
            if (!isVariant)
            {
                throw new DomainException(E.GroupAttrMustBeVariant(key));
            }
            if (isRequired)
            {
                throw new DomainException(E.GroupAttrMustNotBeRequired(key));
            }
        }

        if (kind is AttributeKind.Lookup)
        {
            if (lookupType is null)
            {
                throw new DomainException(E.LookupTypeRequired);
            }

            if (Attributes
                .OfType<LookupAttributeDefinition>()
                .Any(a => a.LookupType == lookupType))
            {
                throw new DomainException(E.DuplicateLookupTypeOnProductType([lookupType.Key]));
            }
        }

        AttributeDefinition def = kind switch
        {
            AttributeKind.Int => new IntAttributeDefinition(this, key, isRequired, isVariant, position, unit, (int?)min, (int?)max, (int?)step),
            AttributeKind.Decimal => new DecimalAttributeDefinition(this, key, isRequired, isVariant, position, unit, min, max, step),
            AttributeKind.String => new StringAttributeDefinition(this, key, isRequired, position, regex),
            AttributeKind.Bool => new BoolAttributeDefinition(this, key, isRequired, position),
            AttributeKind.Date => new DateAttributeDefinition(this, key, isRequired, position),
            AttributeKind.Enum => new EnumAttributeDefinition(this, key, isRequired, isVariant, position),
            AttributeKind.Group => new GroupAttributeDefinition(this, key, position),
            AttributeKind.Lookup => new LookupAttributeDefinition(this, key, isRequired, isVariant, position, lookupType),
            _ => throw new UnreachableException(),
        };

        Attributes.Add(def);
        return def;
    }

    /// <summary>
    /// Defines a dependent attribute that is linked to an existing parent attribute of enumeration or lookup type.
    /// </summary>
    /// <param name="parentKey">The key of the parent attribute to which the new dependent attribute will be linked. The parent attribute must
    /// be of enumeration or lookup type.</param>
    /// <param name="key">The unique key for the attribute.</param>
    /// <param name="kind">The kind of the attribute. Must be either enumeration or lookup type.</param>
    /// <param name="isRequired">Indicates whether the attribute is required.</param>
    /// <param name="position">The position of the attribute in the attribute list.</param>
    /// <param name="isVariant">Indicates whether this attribute's value creates a unique, sellable variant of a listing.</param>
    /// <param name="lookupType">The lookup type associated with the attribute if applicable. Only applicable for lookup attributes.</param>
    public DependentAttributeDefinition DefineDependentAttribute(
        string parentKey,
        string key,
        AttributeKind kind,
        bool isRequired,
        bool isVariant,
        int position,
        LookupType? lookupType = null)
    {
        if (Attributes.SingleOrDefault(a => a.Key == parentKey) is not AttributeDefinition parent)
        {
            throw new DomainException(E.AttrNotFound(parentKey));
        }
        if (parent is not DependentAttributeDefinition enumParent)
        {
            throw new DomainException(E.AttrNotEnum(parentKey));
        }

        var child = (DependentAttributeDefinition)DefineAttribute(key, kind, isRequired, isVariant, position, lookupType: lookupType);
        child.SetDependency(enumParent);
        return child;
    }

    /// <summary>
    /// Removes the attribute with the specified key from the list of attributes.
    /// </summary>
    /// <param name="key">The key of the attribute to remove. Cannot be null.</param>
    public void RemoveAttribute(string key)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (Attributes.SingleOrDefault(a => a.Key == key) is not { } attr)
        {
            throw new DomainException(E.AttrNotFound(key));
        }

        if (attr is DependentAttributeDefinition depAttr)
        {
            // Ensure no other attribute depends on this one
            var dependents = Attributes
                .OfType<DependentAttributeDefinition>()
                .Where(a => a.DependsOn == attr)
                .Select(a => a.Key)
                .ToArray();

            if (dependents.Length > 0)
            {
                throw new DomainException(E.RemoveForbiddenHasDependants(key, dependents));
            }
        }

        // Remove associated allowed lookup entries
        if (attr is LookupAttributeDefinition lookupAttr)
        {
            LookupsAllowed.RemoveAll(la => la.Option.Type == lookupAttr.LookupType);
        }

        Attributes.Remove(attr);
    }

    /// <summary>
    /// Removes the dependency from the attribute definition identified by the specified key.
    /// </summary>
    /// <param name="key">The key of the attribute definition whose dependency is to be cleared.</param>
    public void ClearAttributeDependency(string key)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (Attributes.SingleOrDefault(a => a.Key == key) is not EnumAttributeDefinition attr)
        {
            throw new DomainException(E.EnumAttrNotFound(key));
        }

        attr.ClearDependency();
    }

    /// <summary>
    /// Adds a new option to the specified enum attribute definition.
    /// </summary>
    /// <param name="key">The unique key identifying the attribute to which the option will be added. Must correspond to an existing enum
    /// attribute definition.</param>
    /// <param name="optionCode">The unique code for the new option to add.</param>
    /// <param name="position">The zero-based position at which to insert the new option within the attribute's option list.</param>
    /// <param name="parentOptionCode">The code of the parent option under which to nest the new option, or null to add the option at the root level.</param>
    public EnumAttributeOption AddAttributeOption(
        string key,
        string optionCode,
        int position,
        string? parentOptionCode = null)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (Attributes.SingleOrDefault(a => a.Key == key) is not EnumAttributeDefinition attr)
        {
            throw new DomainException(E.EnumAttrNotFound(key));
        }

        return attr.AddOption(optionCode, position, parentOptionCode);
    }

    public void AddGroupAttributeMember(
        string groupKey,
        string memberKey)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }
        if (Attributes.SingleOrDefault(a => a.Key == groupKey) is not GroupAttributeDefinition groupAttr)
        {
            throw new DomainException(E.GroupAttrNotFound(groupKey));
        }
        if (Attributes.SingleOrDefault(a => a.Key == memberKey) is not NumericAttributeDefinition numericAttr)
        {
            throw new DomainException(E.NumericAttrNotFound(memberKey));
        }

        groupAttr.AddMember(numericAttr);
    }

    /// <summary>
    /// Adds the specified lookup option to the list of allowed lookups for this product type, enforcing ancestor
    /// allow-list constraints.
    /// </summary>
    /// <remarks>This method ensures that the allow-list for a child product type is always a subset of its
    /// nearest ancestor's allow-list for the same lookup type. Attempting to add an option not allowed by an ancestor
    /// will result in an exception.</remarks>
    /// <param name="option">The lookup option to add to the allow-list. The option's type must be permitted by the nearest
    /// ancestor's allow-list, if one exists.</param>
    public void AddAllowedLookup([NotNull] LookupOption option)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        // Ensure the option is a member of the ancestor's allow-list

        if (TryGetNearestAllowedSet(includeSelf: false, option.Type, out var owner, out var ancestorSet) &&
            !ancestorSet.Contains(option))
        {
            throw new DomainException(E.LookupOptsNotAllowedByAncestor([option.Code], option.Type.Key, owner.SlugPath));
        }

        // The lookup option is either a subset of the ancestor's allow-list or there is no ancestor with entries for this type
        // making this the topmost node.

        // prevent local duplicates
        if (LookupsAllowed.Any(a => a.Option == option))
        {
            throw new DomainException(E.DuplicateAllowedLookupOpts([option.Code]));
        }

        LookupsAllowed.Add(new LookupAllowed(this, option));
    }

    public void RemoveAllowedLookup([NotNull] LookupOption value)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (LookupsAllowed.SingleOrDefault(a => a.Option == value) is not { } existing)
        {
            throw new DomainException(E.LookupOptNotFound(value.Code));
        }

        LookupsAllowed.Remove(existing);
    }

    /// <summary>
    /// Transitions the product type from the draft state to the published state.
    /// </summary>
    public void Publish()
    {
        if (State is ProductTypeState.Published)
        {
            throw new DomainException(E.AlreadyPublished);
        }
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        ValidateForPublish();
        State = ProductTypeState.Published;
        // set PublishedAt & schemaHash, etc.
    }

    /// <summary>
    /// Returns true if the speicifed lookup option is permitted by this product type or any ancestor that declares an allow-list
    /// for that value's type. Nearest ancestor with entries wins. If no ancestor declares entries for that value, the fallback policy
    /// applies as set in noEntriesMeansAllowAll.
    /// </summary>
    /// <param name="option">The lookup option to check.</param>
    /// <param name="noEntriesMeansAllowAll">If true, the absence of any allow-list entries for the option's type in the product type lineage means all options of that type are allowed.</param>
    /// <returns></returns>
    public bool IsLookupOptionAllowed(
        [NotNull] LookupOption option,
        bool noEntriesMeansAllowAll)
    {
        if (TryGetNearestAllowedSet(includeSelf: true, option.Type, out _, out var allowedSet))
        {
            return allowedSet.Contains(option);
        }

        // No node declared entries for this type
        return noEntriesMeansAllowAll;
    }

    private void ValidateForPublish()
    {
        // Acyclic dependency check
        AttributeSchemaUtils.EnsureAcyclic(Attributes);

        // All attribute definitions must have unique positions to ensure deterministic ordering
        var posSet = new HashSet<int>();
        foreach (var attr in Attributes)
        {
            if (!posSet.Add(attr.Position))
                    {
                throw new DomainException(E.DuplicateAttrPosition(attr.Key));
                    }

            attr.Validate();
        }

        var lookupDefs = Attributes.OfType<LookupAttributeDefinition>().ToArray();
        var usedLookupTypes = lookupDefs.Select(a => a.LookupType).ToHashSet();

        // Ensure no lookup attributes without allow-list when required by ConstraintMode
        foreach (var la in lookupDefs)
        {
            if (la.Config.ConstraintMode is LookupConstraintMode.RequireAllowList &&
                !TryGetNearestAllowedSet(includeSelf: true, la.LookupType, out _, out _))
            {
                throw new DomainException(E.MissingLookupAllowList(la.Key, la.LookupType.Key));
            }
        }

        // Stale types: allow-list entries whose type isn't in the schema
        var staleLookupTypeKeys = LookupsAllowed
            .Where(la => !usedLookupTypes.Contains(la.Option.Type))
            .Select(la => la.Option.Type.Key)
            .Distinct()
            .ToArray();

        if (staleLookupTypeKeys.Length > 0)
        {
            throw new DomainException(E.AllowListContainsLookupTypesNotInSchema(staleLookupTypeKeys));
        }

        // Ensure no duplicate lookup types across attributes (at most one attr per type)
        var duplicateLookupTypeKeys = lookupDefs
            .GroupBy(a => a.LookupType)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key.Key)
            .ToArray();

        if (duplicateLookupTypeKeys.Length > 0)
        {
            throw new DomainException(E.DuplicateLookupTypeOnProductType(duplicateLookupTypeKeys));
        }

        // Ensure no duplicate options in allow-list (same option added more than once)
        var duplicateAllowedOptions = LookupsAllowed
            .GroupBy(a => a.Option)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key.Code)
            .ToArray();

        if (duplicateAllowedOptions.Length > 0)
        {
            throw new DomainException(E.DuplicateAllowedLookupOpts(duplicateAllowedOptions));
        }

        // Build local map: type -> {values}
        var localAllowedByType = LookupsAllowed
            .GroupBy(a => a.Option.Type)
            .ToDictionary(g => g.Key, g => g.Select(a => a.Option).ToHashSet());

        // Subset-of-nearest-ancestor: if this PT declares entries for a type,
        // they must be a subset of the nearest ancestor's entries for that type (if any).
        foreach (var (lookupType, localSet) in localAllowedByType)
        {
            if (!TryGetNearestAllowedSet(includeSelf: false, lookupType, out var ancestor, out var ancestorSet))
            {
                // No ancestor constraint
                continue;
            }

            if (!localSet.IsSubsetOf(ancestorSet))
            {
                var offendingKeys = LookupsAllowed
                    .Where(a => a.Option.Type == lookupType && !ancestorSet.Contains(a.Option))
                    .Select(a => a.Option.Code)
                    .Distinct()
                    .ToArray();

                throw new DomainException(E.LookupOptsNotAllowedByAncestor(offendingKeys, lookupType.Key, ancestor.SlugPath));
            }
        }

        //// disallow deprecated/inactive items at publish time
        //var invalidItems = AllowedLookups
        //    .Where(la => la.Value.Status != LookupStatus.Active)
        //    .Select(p => p.Value.Type)
        //    .ToArray();

        //if (invalidItems.Length > 0)
        //{
        //    throw new DomainException(E.AllowListContainsInactiveValues(invalidItems));
        //}
    }

    private bool TryGetNearestAllowedSet(
        bool includeSelf,
        LookupType lookupType,
        [NotNullWhen(true)] out ProductType? owner,
        out HashSet<LookupOption> allowSet)
    {
        owner = null;
        allowSet = [];

        // Start from self -or- parent and walk up the ancestor chain
        // Find nearest node (including self, if set) that has entries for the value's lookup type
        foreach (var t in BuildChain(reverse: true, includeSelf: includeSelf))
        {
            if (t.LookupsAllowed.Any(a => a.Option.Type == lookupType))
            {
                owner = t;
                break;
            }
        }

        // If lookup type is found, return all options of that type from the owner node

        if (owner is not null)
        {
            allowSet = [.. owner.LookupsAllowed
                .Where(a => a.Option.Type == lookupType)
                .Select(a => a.Option)];

            return true;
        }

        return false;
    }

    // Build ancestor chain from root to this (self inclusive)
    private List<ProductType> BuildChain(bool reverse = false, bool includeSelf = true)
    {
        var chain = new List<ProductType>();
        CollectAncestors(this);

        if (reverse)
        {
            chain.Reverse();
        }

        return chain;

        void CollectAncestors(ProductType node)
        {
            if (node.Parent is not null)
            {
                CollectAncestors(node.Parent);
            }

            if (includeSelf || node != this)
            {
                chain.Add(node);
            }
        }
    }

    private string DebuggerDisplay => $"{SlugPath} ({Kind}) | (v{Version}, {State})";
}
