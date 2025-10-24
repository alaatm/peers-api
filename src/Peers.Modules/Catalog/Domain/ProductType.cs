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
[DebuggerDisplay("{D,nq}")]
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
        try
        {
            ValidateLookupAttrDependencyRequirements();
        }
        catch
        {
            // rollback
            Attributes.Remove(def);
            throw;
        }

        return def;
    }

    /// <summary>
    /// Defines a dependent attribute that is linked to an existing parent attribute of enum type.
    /// </summary>
    /// <param name="parentKey">The key of the parent attribute to which the new dependent attribute will be linked. The parent attribute must
    /// be of enum type.</param>
    /// <param name="key">The unique key for the attribute.</param>
    /// <param name="isRequired">Indicates whether the attribute is required.</param>
    /// <param name="position">The position of the attribute in the attribute list.</param>
    /// <param name="isVariant">Indicates whether this attribute's value creates a unique, sellable variant of a listing.</param>
    public EnumAttributeDefinition DefineDependentAttribute(
        string parentKey,
        string key,
        bool isRequired,
        bool isVariant,
        int position)
    {
        if (Attributes.SingleOrDefault(a => a.Key == parentKey) is not AttributeDefinition parent)
        {
            throw new DomainException(E.AttrNotFound(parentKey));
        }
        if (parent is not EnumAttributeDefinition enumParent)
        {
            throw new DomainException(E.AttrNotEnum(parentKey));
        }

        var child = (EnumAttributeDefinition)DefineAttribute(key, AttributeKind.Enum, isRequired, isVariant, position);
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

        if (attr is EnumAttributeDefinition depAttr)
        {
            // Ensure no other attribute depends on this one
            var dependents = Attributes
                .OfType<EnumAttributeDefinition>()
                .Where(a => a.DependsOn == attr)
                .Select(a => a.Key)
                .ToArray();

            if (dependents.Length > 0)
            {
                throw new DomainException(E.RemoveForbiddenHasDependants(key, dependents));
            }
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
    /// <param name="key">The unique key identifying the attribute to which the option will be added. Must correspond to an existing
    /// enum attribute definition.</param>
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
    /// Adds the specified lookup option to the list of allowed lookups for the specified attribute.
    /// </summary>
    /// <remarks>This method ensures that the allow-list for a child product type is always a subset of its
    /// nearest ancestor's allow-list for the same lookup type. Attempting to add an option not allowed by an ancestor
    /// will result in an exception.</remarks>
    /// <param name="key">The unique key identifying the attribute to which the option will be added to the allow list. Must correspond to an existing
    /// lookup attribute definition.</param>
    /// <param name="option">The lookup option to add to the allow-list. The option's type must be permitted by the nearest
    /// ancestor's allow-list, if one exists.</param>
    public void AddAllowedLookup(
        string key,
        [NotNull] LookupOption option)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (Attributes.SingleOrDefault(a => a.Key == key) is not LookupAttributeDefinition attr)
        {
            throw new DomainException(E.LookupAttrNotFound(key));
        }

        if (!HasUsableParentPath())
        {
            throw new DomainException(E.NoUsableLookupParentPath(option.Code, option.Type.Key));
        }
        if (IsDeadForAnyChild())
        {
            throw new DomainException(E.DeadLookupOptionForChild(option.Code, option.Type.Key));
        }

        attr.AddAllowedOption(option);

        bool HasUsableParentPath()
        {
            // Find parent attributes on this product type that actually gate this child type
            var parents = Attributes
                .OfType<LookupAttributeDefinition>()
                .Where(p => p.LookupType.ParentLinks.Any(p => p.ChildType == option.Type))
                .ToArray();

            // If there's no parent attribute present here, this child option is usable
            if (parents.Length == 0)
            {
                return true;
            }

            foreach (var parent in parents)
            {
                // Allowed parent options for this attribute (empty => "allow all" on this attribute)
                var allowedParents = parent.AllowedOptions.Count != 0
                    ? [.. parent.AllowedOptions.Select(p => p.Option)]
                    : parent.LookupType.Options;

                // Does ANY allowed parent option link to this child option?
                foreach (var allowedParent in allowedParents)
                {
                    var hasLink = parent.LookupType.ParentLinks.Any(p =>
                        p.ChildType == option.Type &&
                        p.ChildOption == option &&
                        p.ParentOption == allowedParent);

                    if (hasLink)
                    {
                        // This parent option can reach the child option via an allowed parent option
                        return true;
                    }
                }
            }

            // No parent on this PT can reach the child option via an allowed parent option
            return false;
        }

        bool IsDeadForAnyChild()
        {
            // Children on THIS product type that are gated by this option's type
            var children = Attributes
                .OfType<LookupAttributeDefinition>()
                .Where(p => p.LookupType.ChildLinks.Any(p => p.ParentType == option.Type))
                .ToArray();

            // If there are no linked children on this PT, nothing to check
            if (children.Length == 0)
            {
                return false;
            }

            foreach (var child in children)
            {
                // Allowed child options for this child attribute (empty => "allow all" on this attribute)
                var allowedChildren = child.AllowedOptions.Count != 0
                    ? [.. child.AllowedOptions.Select(p => p.Option)]
                    : child.LookupType.Options;

                // Does this parent option link to ANY allowed child option of this child attribute?
                var hasAny = child.LookupType.ChildLinks.Any(p =>
                    p.ParentType == option.Type &&
                    p.ParentOption == option &&
                    allowedChildren.Contains(p.ChildOption));

                if (!hasAny)
                {
                    // This parent option would be "dead" for this child attribute
                    return true;
                }
            }

            return false;
        }
    }

    //public void RemoveAllowedLookup([NotNull] LookupOption value)
    //{
    //    if (State is not ProductTypeState.Draft)
    //    {
    //        throw new DomainException(E.NotDraft);
    //    }

    //    if (LookupsAllowed.SingleOrDefault(a => a.Option == value) is not { } existing)
    //    {
    //        throw new DomainException(E.LookupOptNotFound(value.Code));
    //    }

    //    LookupsAllowed.Remove(existing);
    //    EffectiveLookupsAllowed = null!; // reset cache
    //}

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

    private void ValidateLookupAttrDependencyRequirements()
    {
        var lookupAttrs = Attributes
            .OfType<LookupAttributeDefinition>()
            .ToArray();

        foreach (var child in lookupAttrs)
        {
            var linkedParents = lookupAttrs
                .Where(p => p.LookupType.ParentLinks.Any(pl => pl.ChildType == child.LookupType))
                .ToArray();

            foreach (var parent in linkedParents)
            {
                if (parent.IsVariant && !child.IsVariant)
                {
                    throw new DomainException(E.VariantDependencyViolation(childKey: child.Key, parentKey: parent.Key));
                }
                if (child.IsRequired && !parent.IsRequired)
                {
                    throw new DomainException(E.RequiredDependencyViolation(childKey: child.Key, parentKey: parent.Key));
                }
            }
        }
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

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"PT:{Id} - {SlugPath} ({Kind}) | (v{Version}, {State})";
}
