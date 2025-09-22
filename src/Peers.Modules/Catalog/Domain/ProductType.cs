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
    /// The list of allowed lookup values for lookup attributes in this product type.
    /// </summary>
    public List<LookupAllowed> LookupAllowedList { get; private set; } = default!;
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
        LookupAllowedList = [];
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

        var childSlug = SlugHelper.ToSlug(name);

        if (Children.Any(p => p.Slug == childSlug))
        {
            throw new DomainException(E.ChildAlreadyExists(name.Trim()));
        }

        var child = new ProductType(Kind, childSlug, $"{Slug}/{childSlug}", isSelectable, version ?? Version);
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
        if (State is not ProductTypeState.Published)
        {
            throw new DomainException(E.NotPublished);
        }

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

            // Copy LookupAllowed entries only if attributes were copied
            foreach (var la in LookupAllowedList)
            {
                next.LookupAllowedList.Add(new LookupAllowed(next, la.Value));
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
    /// <param name="isVariant">Indicates whether this enum attribute's value creates a unique, sellable variant of a listing.</param>
    /// <param name="lookupType">The lookup type associated with the lookup attribute if applicable.</param>
    /// <param name="unit">The unit of measurement for the attribute if applicable.</param>
    /// <param name="minInt">The minimum value for integer attributes if applicable.</param>
    /// <param name="maxInt">The maximum value for integer attributes if applicable.</param>
    /// <param name="minDecimal">The minimum value for decimal attributes if applicable.</param>
    /// <param name="maxDecimal">The maximum value for decimal attributes if applicable.</param>
    /// <param name="regex">A regular expression pattern for string attributes if applicable.</param>
    public AttributeDefinition DefineAttribute(
        string key,
        AttributeKind kind,
        bool isRequired,
        int position,
        bool isVariant = false,
        LookupType? lookupType = null,
        string? unit = null,
        int? minInt = null,
        int? maxInt = null,
        decimal? minDecimal = null,
        decimal? maxDecimal = null,
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

        if (isVariant && kind is not AttributeKind.Enum)
        {
            throw new DomainException(E.VariantReqEnum);
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
            AttributeKind.Int => new IntAttributeDefinition(this, key, isRequired, position, unit, minInt, maxInt),
            AttributeKind.Decimal => new DecimalAttributeDefinition(this, key, isRequired, position, unit, minDecimal, maxDecimal),
            AttributeKind.String => new StringAttributeDefinition(this, key, isRequired, position, regex),
            AttributeKind.Bool => new BoolAttributeDefinition(this, key, isRequired, position),
            AttributeKind.Date => new DateAttributeDefinition(this, key, isRequired, position),
            AttributeKind.Enum => new EnumAttributeDefinition(this, key, isRequired, position, isVariant),
            AttributeKind.Lookup => new LookupAttributeDefinition(this, key, isRequired, position, lookupType),
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
    /// <param name="isVariant">Indicates whether this attribute's value creates a unique, sellable variant of a listing. Only applicable for
    /// enumeration attributes.</param>
    /// <param name="lookupType">The lookup type associated with the attribute if applicable. Only applicable for lookup attributes.</param>
    public DependentAttributeDefinition DefineDependentAttribute(
        string parentKey,
        string key,
        AttributeKind kind,
        bool isRequired,
        int position,
        bool isVariant = false,
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

        var child = (DependentAttributeDefinition)DefineAttribute(key, kind, isRequired, position, isVariant: isVariant, lookupType: lookupType);
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

        // Remove associated LookupAllowed entries
        if (attr is LookupAttributeDefinition lookupAttr)
        {
            LookupAllowedList.RemoveAll(la => la.Value.Type == lookupAttr.LookupType);
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
    /// <param name="attributeKey">The unique key identifying the attribute to which the option will be added. Must correspond to an existing enum
    /// attribute definition.</param>
    /// <param name="optionKey">The unique key for the new option to add.</param>
    /// <param name="position">The zero-based position at which to insert the new option within the attribute's option list.</param>
    /// <param name="parentOptionKey">The key of the parent option under which to nest the new option, or null to add the option at the root level.</param>
    public EnumAttributeOption AddAttributeOption(
        string attributeKey,
        string optionKey,
        int position,
        string? parentOptionKey = null)
    {
        if (State is not ProductTypeState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (Attributes.SingleOrDefault(a => a.Key == attributeKey) is not EnumAttributeDefinition attr)
        {
            throw new DomainException(E.EnumAttrNotFound(attributeKey));
        }

        return attr.AddOption(optionKey, position, parentOptionKey);
    }

    /// <summary>
    /// Transitions the product type from the draft state to the published state.
    /// </summary>
    public void Publish()
    {
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

        // Consistency: if DependsOn set => all options must be scoped; if null => none may be scoped
        foreach (var attr in Attributes.OfType<EnumAttributeDefinition>())
        {
            if (attr.DependsOn is { } parentAttr)
            {
                if (attr.Options.Any(o => o.ParentOption is null))
                {
                    throw new DomainException(E.DepScopeReq(attr.Key, parentAttr.Key));
                }

                foreach (var opt in attr.Options)
                {
                    Debug.Assert(opt.ParentOption is not null);

                    if (opt.ParentOption.EnumAttributeDefinition != parentAttr)
                    {
                        throw new DomainException(E.InvalidScopeParent(attr.Key, parentAttr.Key, opt.Key, opt.ParentOption.EnumAttributeDefinition.Key));
                    }
                }
            }
            else if (attr.Options.Any(o => o.ParentOption is not null))
            {
                throw new DomainException(E.ScopeForbiddenWithoutDep(attr.Key));
            }
        }

        // Assert no allow-list row for a lookup-type that isn't in the schema.

        // types used by attributes
        var usedLookupTypes = Attributes
            .OfType<LookupAttributeDefinition>()
            .Select(a => a.LookupType)
            .ToHashSet();

        // Stale rows: allow-list entries whose lookup-type is not present anymore
        var staleLookupTypes = LookupAllowedList
            .Where(la => !usedLookupTypes.Contains(la.Value.Type))
            .Select(la => la.Value.Type.Key)
            .ToArray();

        if (staleLookupTypes.Length > 0)
        {
            throw new DomainException(E.AllowListContainsLookupTypesNotInSchema(staleLookupTypes));
        }

        // Ensure no two or more lookup attributes use the same lookup type (entity equality)
        var duplicateLookupTypes = Attributes
            .OfType<LookupAttributeDefinition>()
            .GroupBy(a => a.LookupType)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key.Key)
            .ToArray();

        if (duplicateLookupTypes.Length > 0)
        {
            throw new DomainException(E.DuplicateLookupTypeOnProductType(duplicateLookupTypes));
        }

        //// disallow deprecated/inactive items at publish time
        //var invalidItems = LookupAllowedList
        //    .Where(la => la.Value.Status != LookupStatus.Active)
        //    .Select(p => p.Value.Type)
        //    .ToArray();

        //if (invalidItems.Length > 0)
        //{
        //    throw new DomainException(E.AllowListContainsInactiveValues(invalidItems));
        //}
    }

    private string DebuggerDisplay => $"{SlugPath} ({Kind}) | (v{Version}, {State})";
}
