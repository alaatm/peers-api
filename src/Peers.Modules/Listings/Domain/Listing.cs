using System.Diagnostics;
using NetTopologySuite.Geometries;
using Peers.Core.Domain.Errors;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Listings.Domain.Translations;
using static Peers.Modules.Listings.Commands.SetAttributes.Command;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Represents a product listing created by a seller, including its attributes, variants, pricing, and associated
/// product type information.
/// </summary>
/// <remarks>A listing defines the details of a product offered by a seller, such as its title, description, base
/// price, and available variants. It is associated with a specific product type and maintains both non-variant
/// attributes and SKU-level variant information. The listing's state and order quantity policy control its availability
/// and purchasing constraints.</remarks>
public sealed class Listing : Entity, IAggregateRoot, ILocalizable<Listing, ListingTr>
{
    /// <summary>
    /// The version number of the listing.
    /// </summary>
    /// <remarks>
    /// Each update post-publish must increments this and recalculates snapshots of all variant axes
    /// </remarks>
    public int Version { get; private set; }
    /// <summary>
    /// The identifier of the seller who created the listing.
    /// </summary>
    public int SellerId { get; private set; }
    /// <summary>
    /// The date and time when the listing was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// The date and time when the listing was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }
    /// <summary>
    /// A unique hashtag for the listing, used for easy reference and sharing.
    /// </summary>
    public string? Hashtag { get; private set; }
    /// <summary>
    /// The title of the listing.
    /// </summary>
    public string Title { get; private set; } = default!;
    /// <summary>
    /// The description of the listing.
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// The base price of the listing.
    /// </summary>
    public decimal BasePrice { get; private set; }
    /// <summary>
    /// The current state of the listing.
    /// </summary>
    public ListingState State { get; private set; }
    /// <summary>
    /// The identifier of the product type associated with this listing.
    /// </summary>
    public int ProductTypeId { get; private set; }
    /// <summary>
    /// The version of the product type at the time the listing was created.
    /// </summary>
    public int ProductTypeVersion { get; private set; }
    /// <summary>
    /// A snapshot of the variant axes defined for the listing at the time of its last update.
    /// </summary>
    public VariantAxesSnapshot AxesSnapshot { get; private set; } = default!;
    /// <summary>
    /// The fulfillment preferences for the listing, indicating how orders will be fulfilled.
    /// </summary>
    public FulfillmentPreferences FulfillmentPreferences { get; private set; } = default!;
    /// <summary>
    /// The seller who created the listing.
    /// </summary>
    public Customer Seller { get; private set; } = default!;
    /// <summary>
    /// The product type associated with this listing.
    /// </summary>
    public ProductType ProductType { get; private set; } = default!;
    /// <summary>
    /// The list of non-variant attributes of the listing.
    /// </summary>
    public List<ListingAttribute> Attributes { get; private set; } = default!;
    /// <summary>
    /// The list of SKU-level variants of the listing.
    /// </summary>
    public List<ListingVariant> Variants { get; private set; } = default!;
    /// <summary>
    /// The list of translations associated with this listing.
    /// </summary>
    public List<ListingTr> Translations { get; private set; } = default!;

    /// <summary>
    /// Creates a new draft listing for the specified seller and product type with the provided details.
    /// </summary>
    /// <param name="seller">The customer who will be the seller of the listing.</param>
    /// <param name="productType">The product type to associate with the listing. Must be published and selectable.</param>
    /// <param name="title">The title of the listing.</param>
    /// <param name="description">An optional description of the listing.</param>
    /// <param name="hashtag">An optional unique hashtag for the listing.</param>
    /// <param name="price">The base price for the listing.</param>
    /// <param name="date">The creation date and time for the listing.</param>
    public static Listing Create(
        [NotNull] Customer seller,
        [NotNull] ProductType productType,
        [NotNull] string title,
        string? description,
        string? hashtag,
        decimal price,
        DateTime date)
    {
        const int DefaultVersion = 1;

        if (productType.State is not ProductTypeState.Published)
        {
            throw new DomainException(E.ProductTypeNotPublished(productType.SlugPath));
        }

        if (!productType.IsSelectable)
        {
            throw new DomainException(E.ProductTypeNotSelectable(productType.SlugPath));
        }

        Point? originLocation = null;
        if (productType.Kind is ProductTypeKind.Physical)
        {
            if (seller.GetDefaultAddress() is not { } address)
            {
                throw new DomainException(E.SellerMustHaveAddress);
            }

            originLocation = address.Location;
        }

        return new()
        {
            Version = DefaultVersion,
            AxesSnapshot = VariantAxesSnapshot.Create(DefaultVersion),
            FulfillmentPreferences = FulfillmentPreferences.Default(originLocation),
            Seller = seller,
            ProductType = productType,
            ProductTypeVersion = productType.Version,
            CreatedAt = date,
            UpdatedAt = date,
            Title = title.Trim(),
            Description = description?.Trim(),
            Hashtag = hashtag?.Trim(),
            BasePrice = price,
            State = ListingState.Draft,
            Attributes = [],
            Variants = [],
        };
    }

    /// <summary>
    /// Configures the listing's attributes and variants based on the provided attribute inputs and variant axis constraints.
    /// </summary>
    /// <remarks>
    /// All required attributes defined in the product type schema must be present in the inputs. The current set of attributes and variants
    /// for the listing are replaced. This operation can only be performed when the listing is in the draft state.
    /// </remarks>
    /// <param name="inputs">A dictionary containing attribute keys and their corresponding input values. Each key must exist in the product
    /// type schema, and each value must be non-null.</param>
    /// <param name="variantAxesCap">The maximum number of variant axes allowed for the listing. Must be greater than or equal to 1.</param>
    /// <param name="skuCap">The maximum number of SKUs (variants) allowed for the listing. Must be greater than or equal to 1.</param>
    public void SetAttributes(
        [NotNull] Dictionary<string, AttributeInputDto> inputs,
        int variantAxesCap,
        int skuCap)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(variantAxesCap, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(skuCap, 1);

        if (State is not ListingState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        // Resolve all PT defs by key (single source of truth)
        var defsByKey = ProductType
            .Attributes
            .ToDictionary(a => a.Key, StringComparer.Ordinal);

        // Ensure all keys on provided inputs exist in the schema and have non-null values
        foreach (var (key, value) in inputs)
        {
            if (!defsByKey.ContainsKey(key))
            {
                throw new DomainException(E.AttrNotDefined(key, ProductType.SlugPath));
            }
            if (value is null)
            {
                throw new DomainException(E.AttrValueCannotBeNull(key));
            }
        }

        // Ensure all required attributes are present in the inputs
        foreach (var (key, value) in defsByKey)
        {
            if (value.IsRequired && !inputs.ContainsKey(key))
            {
                throw new DomainException(E.AttrReq(key));
            }
        }

        // Build non-variant attributes (and validate requireds)
        var headerAttrs = BuildNonVariantAttributes(inputs, defsByKey);

        // Build variant axes (independent + composite)
        var axes = BuildVariantAxes(
            inputs,
            defsByKey,
            variantAxesCap,
            skuCap);

        // Generate variants
        var newVariants = ListingVariantsFactory.GenerateVariants(
            this,
            axes,
            out var axesSnaptshot);

        // Replace state
        Variants.Clear();
        Variants.AddRange(newVariants);
        AxesSnapshot = axesSnaptshot;

        Attributes.Clear();
        Attributes.AddRange(headerAttrs);

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Appends new variant choices to existing variant axes for the listing, subject to SKU capacity and axis
    /// constraints.
    /// </summary>
    /// <remarks>
    /// This method only allows appending new choices to axes that already exist on the listing; it
    /// does not permit introducing new axes after the listing is published. The SKU cap is strictly enforced, and no
    /// variants will be added if the operation would exceed the allowed limit. The axes snapshot and variant list are
    /// updated upon successful append.
    /// Header attributes (non-variant) are ignored.
    /// </remarks>
    /// <param name="inputs">A dictionary containing attribute input data for each axis to append. Each key represents an axis identifier,
    /// and the value provides the choices to be added. Cannot be null.</param>
    /// <param name="skuCap">The maximum allowed number of SKUs after appending new variants. Must be greater than or equal to the current
    /// SKU count.</param>
    public void AppendVariantAxes(
        [NotNull] Dictionary<string, AttributeInputDto> inputs,
        int skuCap)
    {
        if (State is ListingState.Draft)
        {
            throw new DomainException(E.AppendOnlyPostPublish);
        }

        // Resolve all PT defs by key (single source of truth)
        var defsByKey = ProductType
            .Attributes
            .ToDictionary(a => a.Key, StringComparer.Ordinal);

        // Ensure all keys on provided inputs exist in the schema and have non-null values
        foreach (var (key, value) in inputs)
        {
            if (!defsByKey.ContainsKey(key))
            {
                throw new DomainException(E.AttrNotDefined(key, ProductType.SlugPath));
            }
            if (value is null)
            {
                throw new DomainException(E.AttrValueCannotBeNull(key));
            }
        }

        // Load the baseline axes from the trusted snapshot (immutable, canonical, sorted)
        var baseline = AxesSnapshot.ToRuntime(ProductType);
        var baselineAxisByKey = baseline.ToDictionary(a => a.Definition.Key, StringComparer.Ordinal);

        // Parse the incoming delta as axes (validated, canonicalized, sorted)
        var deltaAxes = BuildVariantAxes(
            inputs,
            defsByKey,
            variantAxesCap: baseline.Count,         // append cannot introduce new axes
            skuCap: int.MaxValue);                  // final cap enforced later

        if (deltaAxes.Count == 0)
        {
            throw new DomainException(E.AppendRequiresAtLeastOneVariantAxis);
        }

        // Ensure every provided axis already exists on the listing
        foreach (var axis in deltaAxes)
        {
            if (!baselineAxisByKey.ContainsKey(axis.Definition.Key))
            {
                throw new DomainException(E.CannotAddNewAxisPostPublish(axis.Definition.Key));
            }
        }

        // Compute new choices only (per axis)
        var newByAxisKey = new Dictionary<string, List<AxisChoice>>(StringComparer.Ordinal);

        foreach (var delta in deltaAxes)
        {
            var axisKey = delta.Definition.Key;
            var existingChoiceKeys = baselineAxisByKey[axisKey].Choices.Select(c => c.Key).ToHashSet(StringComparer.Ordinal);
            var filtered = new List<AxisChoice>();

            foreach (var c in delta.Choices)
            {
                if (!existingChoiceKeys.Contains(c.Key))
                {
                    // Not in baseline, so it's new
                    filtered.Add(c);
                }
            }

            if (filtered.Count > 0)
            {
                newByAxisKey[axisKey] = filtered;
            }
        }

        if (newByAxisKey.Count == 0)
        {
            // Nothing to append (all provided values already exist)
            throw new DomainException(E.AppendRequiresAtLeastOneNewVariantValue);
        }

        // Build updated baseline (for persistence) by merging new choices, then re-sort per axis canonically
        var updated = new List<VariantAxis>();
        foreach (var axis in baseline)
        {
            if (!newByAxisKey.TryGetValue(axis.Definition.Key, out var add))
            {
                // unchanged axis
                updated.Add(axis);
                continue;
            }

            // Copy existing then add new, then re-sort canonically
            var merged = new List<AxisChoice>(axis.Cardinality + add.Count);
            merged.AddRange(axis.Choices);
            merged.AddRange(add);
            merged.Sort(AxisChoice.Comparison);
            updated.Add(new VariantAxis(axis.Definition, merged));
        }

        // Cap check (upper bound). Every new combo uses >=1 new value by construction.
        var totalUpdated = ListingVariantsFactory.EstimateSkuCount(updated);
        var totalBaseline = ListingVariantsFactory.EstimateSkuCount(baseline);
        var newCount = checked(totalUpdated - totalBaseline);
        if (newCount <= 0)
        {
            throw new DomainException(E.AppendRequiresAtLeastOneNewVariantValue);
        }

        var remaining = Math.Max(0, skuCap - Variants.Count);
        if (newCount > remaining)
        {
            throw new DomainException(E.SkuCapExceeded(skuCap, Variants.Count + newCount));
        }

        Version++;

        // Produce the final updated axes snapshot (new id/version/time) from the updated baseline
        var allVariants = ListingVariantsFactory.GenerateVariants(this, updated, out var newSnapshot);
        var existingKeys = Variants.Select(v => v.VariantKey).ToHashSet(StringComparer.Ordinal);

        var newVariants = new List<ListingVariant>(newCount);
        foreach (var v in allVariants)
        {
            if (!existingKeys.Contains(v.VariantKey))
            {
                newVariants.Add(v);
            }
        }

        // Commit: append variants and persist the new axes snapshot
        Variants.AddRange(newVariants);
        AxesSnapshot = newSnapshot;
        UpdatedAt = DateTime.UtcNow;
    }

    private List<ListingAttribute> BuildNonVariantAttributes(
        Dictionary<string, AttributeInputDto> inputs,
        Dictionary<string, AttributeDefinition> defsByKey)
    {
        var result = new List<ListingAttribute>(inputs.Count);

        foreach (var (key, input) in inputs)
        {
            if (!defsByKey.TryGetValue(key, out var def))
            {
                throw new DomainException(E.AttrNotDefined(key, ProductType.SlugPath)); // defensive; top-level already checks
            }

            // Skip any variant defs (including Group, which are always variant) — handled by BuildVariantAxes
            if (def.IsVariant)
            {
                continue;
            }

            Debug.Assert(def is not GroupAttributeDefinition, "Group defs are always variant.");

            // Group member numerics cannot be set as header attributes; values must come via the group's composite axis
            if (def is NumericAttributeDefinition numDef &&
                numDef.GroupDefinition is not null)
            {
                throw new DomainException(E.GroupMemberCannotBeHeader(def.Key, numDef.GroupDefinition.Key));
            }

            // Reject axis-shaped DTOs for non-variant defs (defense in depth)
            if (input is
                AttributeInputDto.NumericAxis or
                AttributeInputDto.OptionCodeAxis or
                AttributeInputDto.GroupAxis)
            {
                throw new DomainException(E.NonVariantAttrDoesNotAcceptAxis(def.Key));
            }

            // Create the header attribute (each typed helper will validate the scalar DTO shape)
            result.Add(ListingAttribute.Create(this, def, input));
        }

        return result;
    }

    private List<VariantAxis> BuildVariantAxes(
        Dictionary<string, AttributeInputDto> inputs,
        Dictionary<string, AttributeDefinition> defsByKey,
        int variantAxesCap,
        int skuCap)
    {
        var axes = new List<VariantAxis>();

        foreach (var (key, input) in inputs)
        {
            if (!defsByKey.TryGetValue(key, out var def))
            {
                // We already validated all keys exist by the caller.
                Debug.Assert(false, "Key should exist.");
                throw new DomainException(E.AttrNotDefined(key, ProductType.SlugPath));
            }

            // Skip non-variant defs entirely - handled by BuildNonVariantAttributes
            if (!def.IsVariant)
            {
                continue;
            }

            switch (def)
            {
                case EnumAttributeDefinition enumDef:
                    if (input is AttributeInputDto.OptionCodeAxis enumOptAxis)
                    {
                        // Enfore atleast one value per single axis and all values are unique
                        enumOptAxis.Validate(def.Key, unique: true, minRequired: 1);

                        var available = enumDef.Options.ToDictionary(o => o.Code, StringComparer.Ordinal);
                        var picked = new List<AxisChoice>(enumOptAxis.Value.Count);

                        foreach (var code in enumOptAxis.Value)
                        {
                            if (!available.TryGetValue(code, out var opt))
                            {
                                throw new DomainException(E.UnknownEnumAttrOpt(def.Key, code));
                            }

                            picked.Add(new(Key: opt.Code, EnumOption: opt));
                        }

                        picked.Sort(AxisChoice.EnumComparison);
                        axes.Add(new(enumDef, picked));
                    }
                    else
                    {
                        throw new DomainException(E.AxisReqEnumOptAxis(def.Key));
                    }
                    break;
                case LookupAttributeDefinition lookupDef:
                    if (input is AttributeInputDto.OptionCodeAxis lookupOptAxis)
                    {
                        // Enfore atleast one value per single axis and all values are unique
                        lookupOptAxis.Validate(def.Key, unique: true, minRequired: 1);

                        var available = lookupDef.LookupType.Options.ToDictionary(o => o.Code, StringComparer.Ordinal);
                        var picked = new List<AxisChoice>(lookupOptAxis.Value.Count);

                        foreach (var code in lookupOptAxis.Value)
                        {
                            if (!available.TryGetValue(code, out var opt))
                            {
                                throw new DomainException(E.UnknownLookupAttrOpt(def.Key, code));
                            }

                            if (!ProductType.IsLookupOptionAllowed(opt, noEntriesMeansAllowAll: true))
                            {
                                throw new DomainException(E.LookupOptNotAllowedByProductType(code, def.Key, ProductType.SlugPath));
                            }

                            picked.Add(new(Key: opt.Code, LookupOption: opt));
                        }

                        picked.Sort(AxisChoice.LookupComparison);
                        axes.Add(new(lookupDef, picked));
                    }
                    else
                    {
                        throw new DomainException(E.AxisReqLookupOptAxis(def.Key));
                    }
                    break;
                case NumericAttributeDefinition numDef:
                    // Shouldn't happen, as defs cannot be added to a group if they are variant themselves
                    Debug.Assert(numDef.GroupDefinitionId is null && numDef.GroupDefinition is null);

                    if (input is AttributeInputDto.NumericAxis numAxis)
                    {
                        // Enfore atleast one value per single axis and all values are unique
                        numAxis.Validate(def.Key, unique: true, minRequired: 1);

                        var picked = new List<AxisChoice>(numAxis.Value.Count);

                        foreach (var num in numAxis.Value)
                        {
                            numDef.ValidateValue(num);
                            picked.Add(new(Key: num.Normalize(), NumericValue: num));
                        }

                        picked.Sort(AxisChoice.NumericComparison);
                        axes.Add(new(numDef, picked));
                    }
                    else
                    {
                        throw new DomainException(E.AxisReqNumericAxis(def.Key));
                    }
                    break;
                case GroupAttributeDefinition groupDef:
                    if (input is AttributeInputDto.GroupAxis groupAxis)
                    {
                        // Canonical member order (by Position then Key)
                        var membersDefs = groupDef.Members
                            .OrderBy(m => m.Position).ThenBy(m => m.Key, StringComparer.Ordinal)
                            .ToArray();

                        // Enfore atleast one row per group axis and all rows are unique (i.e. prevent [10,20] more than once).
                        groupAxis.Validate(def.Key);

                        var picked = new List<AxisChoice>(groupAxis.Value.Count);

                        foreach (var row in groupAxis.Value)
                        {
                            // Enfore exactly N values per composite axis (N = member count)
                            row.Validate(key, unique: false, exactRequired: membersDefs.Length);

                            var members = new List<AxisChoice.GroupMember>(row.Value.Count);

                            for (var i = 0; i < row.Value.Count; i++)
                            {
                                var memberDef = membersDefs[i];
                                var value = row.Value[i];

                                memberDef.ValidateValue(value);
                                members.Add(new(memberDef, value));
                            }

                            picked.Add(new(
                                Key: string.Join(',', members.Select(p => $"{p.MemberDefinition.Key}={p.Value.Normalize()}")),
                                GroupMembers: members));
                        }

                        // Deterministic order: sort rows lexicographically
                        // i.e.
                        // [ [ 100, 300, 75 ], [ 150, 400, 125 ] ] => [ [ 100, 300, 75 ], [ 150, 400, 125 ] ]
                        // [ [ 150, 400, 125 ], [ 100, 300, 75 ] ] => [ [ 100, 300, 75 ], [ 150, 400, 125 ] ]
                        picked.Sort(AxisChoice.GroupComparison);
                        axes.Add(new(groupDef, picked));
                    }
                    else
                    {
                        throw new DomainException(E.AxisReqMatrix(def.Key));
                    }
                    break;
                default:
                    // Reject any scalar DTO for a variant def, and any axis DTO for a non-supported kind
                    throw new DomainException(E.UnsupportedVariantInput(def.Key, def.Kind));
            }
        }

        // Axis cap
        if (axes.Count > variantAxesCap)
        {
            throw new DomainException(E.VariantAxesCapExceeded(variantAxesCap, axes.Count));
        }
        // Sku cap
        if (ListingVariantsFactory.EstimateSkuCount(axes) is { } skuCount && skuCount > skuCap)
        {
            throw new DomainException(E.SkuCapExceeded(skuCap, skuCount));
        }

        // Sort axes by attribute position then key for deterministic SKU generation
        axes.Sort(VariantAxis.Comparison);

        return axes;
    }

    public void Publish()
    {
        // TODO: Add flag that indicates if the seller has reviewed SKUs prices as they inherit from listing base price by default
        // and fail if set to false
        // -or-
        // set created variants prices to 0 and force seller to update them before publishing (recommended). Base price should only apply to single-default-variant listings.

        if (State is ListingState.Published)
        {
            throw new DomainException(E.AlreadyPublished);
        }
        if (State is not ListingState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        ValidateForPublish();
        State = ListingState.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFulfillmentPreferences([NotNull] FulfillmentPreferences prefs)
    {
        if (State is not ListingState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (prefs.Method is FulfillmentMethod.SellerManaged)
        {
            var offendingSkus = Variants
                .Where(p => p.Logistics is not null)
                .Select(p => p.SkuCode)
                .ToArray();

            if (offendingSkus.Length > 0)
            {
                throw new DomainException(E.CannotSetSellerManagedWhenLogisticsExist(offendingSkus));
            }
        }

        prefs.Validate(ProductType.Kind);
        FulfillmentPreferences = prefs;

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLogistics(string sku, [NotNull] LogisticsProfile profile)
    {
        if (State is not ListingState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (ProductType.Kind is not ProductTypeKind.Physical)
        {
            throw new DomainException(E.LogisticsApplyOnlyToPhysicalListings);
        }

        if (Variants.FirstOrDefault(v => v.SkuCode == sku) is not { } variant)
        {
            throw new DomainException(E.VariantNotFound(sku));
        }

        profile.Validate();
        variant.SetLogistics(profile);
        UpdatedAt = DateTime.UtcNow;
    }

    //public void Archive()
    //{
    //    if (State is ListingState.Archived)
    //    {
    //        throw new DomainException("E.AlreadyArchived");
    //    }

    //    State = ListingState.Archived;
    //    UpdatedAt = DateTime.UtcNow;
    //}

    //public void ActivateVariant(string sku)
    //{
    //    if (Variants.FirstOrDefault(v => v.SkuCode == sku) is not { } variant)
    //    {
    //        throw new DomainException(E.VariantNotFound(sku));
    //    }
    //    variant.Activate();
    //    UpdatedAt = DateTime.UtcNow;
    //}

    //public void DeactivateVariant(string sku)
    //{
    //    if (Variants.FirstOrDefault(v => v.SkuCode == sku) is not { } variant)
    //    {
    //        throw new DomainException(E.VariantNotFound(sku));
    //    }

    //    var activeVariants = Variants.Count(v => v.IsActive);
    //    if (variant.IsActive && activeVariants <= 1)
    //    {
    //        throw new DomainException("E.AtLeastOneActiveVariantRequired");
    //    }

    //    variant.Deactivate();
    //    UpdatedAt = DateTime.UtcNow;
    //}

    /// <summary>
    /// Updates the stock quantity for the variant identified by the specified SKU.
    /// </summary>
    /// <param name="sku">The SKU code of the variant whose stock quantity will be updated. Must correspond to an existing variant.</param>
    /// <param name="newQty">The new stock quantity to set for the variant.</param>
    public void UpdateStockQuantity(string sku, int newQty)
    {
        if (Variants.FirstOrDefault(v => v.SkuCode == sku) is not { } variant)
        {
            throw new DomainException(E.VariantNotFound(sku));
        }

        variant.UpdateStockQuantity(newQty);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the price of the variant identified by the specified SKU code.
    /// </summary>
    /// <param name="sku">The SKU code of the variant whose price will be updated. Must correspond to an existing variant.</param>
    /// <param name="newPrice">The new price to assign to the variant.</param>
    public void UpdatePrice(string sku, decimal newPrice)
    {
        if (Variants.FirstOrDefault(v => v.SkuCode == sku) is not { } variant)
        {
            throw new DomainException(E.VariantNotFound(sku));
        }

        if (variant.VariantKey is ListingVariant.DefaultVariantKey)
        {
            Debug.Assert(Variants.Count == 1);
            BasePrice = newPrice;
        }

        variant.UpdatePrice(newPrice);
        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateForPublish()
    {
        var reqAttrs = ProductType
            .Attributes
            .Where(p => p.IsRequired);

        foreach (var reqAttr in reqAttrs)
        {
            if (!Attributes.Any(p => p.AttributeDefinition == reqAttr))
            {
                throw new DomainException(E.AttrReq(reqAttr.Key));
            }
        }

        var variantAxes = ProductType
            .Attributes
            .OfType<EnumAttributeDefinition>()
            .Where(p => p.IsVariant)
            .ToArray();

        // Ensure at least one option is used for each axis across all variants
        foreach (var axis in variantAxes)
        {
            var usedOpts = Variants
                .SelectMany(p => p.Attributes)
                .Where(p => p.AttributeDefinition == axis)
                .Select(a => a.EnumAttributeOption.Code)
                .Distinct()
                .ToArray();

            if (usedOpts.Length == 0)
            {
                throw new DomainException(E.VariantAttrReqAtleastOneOption(axis.Key));
            }
        }

        if (Variants.Count == 0)
        {
            throw new DomainException(E.AtLeastOneVariantRequired);
        }

        // Ensure each variant sets a value for every axis, and no duplicate combinations
        var comboKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var v in Variants)
        {
            var parts = new List<string>(variantAxes.Length);
            foreach (var axis in variantAxes)
            {
                if (v.Attributes.FirstOrDefault(av => av.AttributeDefinition == axis) is not { } choice)
                {
                    throw new DomainException(E.VariantMissingAxis(axis.Key));
                }

                parts.Add(choice.EnumAttributeOption.Code);
            }

            // If there are no variant axes (simple/default variant), we still want one variant max.
            var key = parts.Count == 0 ? "default" : string.Join("|", parts);
            if (!comboKeys.Add(key))
            {
                throw new DomainException(E.DuplicateVariantCombination);
            }
        }

        // If no axes defined, enforce exactly one "default" variant
        if (variantAxes.Length == 0 && Variants.Count != 1)
        {
            throw new DomainException(E.SingleDefaultVariantExpected);
        }

        // Fulfillment branch

        if (ProductType.Kind is ProductTypeKind.Physical)
        {
            if (FulfillmentPreferences.Method is FulfillmentMethod.None)
            {
                throw new DomainException(E.FulfillmentMethodMustBeSet);
            }

            if (Variants.Any(v => v.Logistics is null))
            {
                throw new DomainException(E.LogisticsRequiredForPhysicalProducts);
            }
        }
        else
        {
            if (FulfillmentPreferences.Method is not FulfillmentMethod.None)
            {
                throw new DomainException(E.FulfillmentMethodMustBeNone);
            }
        }

        FulfillmentPreferences.Validate(ProductType.Kind);
        foreach (var variant in Variants)
        {
            variant.Logistics?.Validate();
        }
    }
}
