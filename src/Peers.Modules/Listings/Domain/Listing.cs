using System.Diagnostics;
using Peers.Core.Domain.Errors;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Listings.Domain.Snapshots;
using Peers.Modules.Listings.Domain.Translations;
using Peers.Modules.Listings.Domain.Validation;
using Peers.Modules.Sellers.Domain;
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
[DebuggerDisplay("{D,nq}")]
public sealed partial class Listing : Entity, IAggregateRoot, ILocalizable<Listing, ListingTr>, IDebuggable
{
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
    /// The identifier of the shipping profile associated with this listing. Only used with seller-managed shipping.
    /// </summary>
    public int? ShippingProfileId { get; private set; }
    /// <summary>
    /// The version of the product type at the time the listing was created.
    /// </summary>
    public int ProductTypeVersion { get; private set; }
    /// <summary>
    /// The current snapshot of the listing, representing its state at a specific point in time.
    /// </summary>
    public ListingSnapshot Snapshot { get; private set; } = default!;
    /// <summary>
    /// The fulfillment preferences for the listing, indicating how orders will be fulfilled.
    /// </summary>
    public FulfillmentPreferences FulfillmentPreferences { get; private set; } = default!;
    /// <summary>
    /// The seller who created the listing.
    /// </summary>
    public Seller Seller { get; private set; } = default!;
    /// <summary>
    /// The product type associated with this listing.
    /// </summary>
    public ProductType ProductType { get; private set; } = default!;
    /// <summary>
    /// The shipping profile associated with this listing. Only used with seller-managed shipping.
    /// </summary>
    public ShippingProfile? ShippingProfile { get; set; }
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

    public bool IsNonShippable =>
        FulfillmentPreferences.Method is FulfillmentMethod.None;

    public bool IsPlatformManagedShipping =>
        FulfillmentPreferences.Method is FulfillmentMethod.PlatformManaged;

    public bool IsSellerManagedShipping =>
        FulfillmentPreferences.Method is FulfillmentMethod.SellerManaged;

    public bool IsSellerManagedNonQuoteBasedShipping =>
        IsSellerManagedShipping &&
        ShippingProfile!.Rate.Kind is not SellerManagedRateKind.Quote;

    public bool IsSellerManagedQuoteBasedShipping =>
        IsSellerManagedShipping &&
        ShippingProfile!.Rate.Kind is SellerManagedRateKind.Quote;

    /// <summary>
    /// Creates a new draft listing for the specified seller and product type with the provided details.
    /// </summary>
    /// <param name="title">The title of the listing.</param>
    /// <param name="seller">The seller of the listing.</param>
    /// <param name="productType">The product type to associate with the listing. Must be published and selectable.</param>
    /// <param name="fulfillment">The fulfillment preferences for the listing.</param>
    /// <param name="shippingProfile">An optional shipping profile for the listing, required if using seller-managed shipping.</param>
    /// <param name="description">An optional description of the listing.</param>
    /// <param name="hashtag">An optional unique hashtag for the listing.</param>
    /// <param name="price">The base price for the listing.</param>
    /// <param name="date">The creation date and time for the listing.</param>
    public static Listing Create(
        [NotNull] string title,
        [NotNull] Seller seller,
        [NotNull] ProductType productType,
        [NotNull] FulfillmentPreferences fulfillment,
        ShippingProfile? shippingProfile,
        string? description,
        string? hashtag,
        decimal price,
        DateTime date)
    {
        if (productType.State is not ProductTypeState.Published)
        {
            throw new DomainException(E.ProductTypeNotPublished(productType.SlugPath));
        }

        if (!productType.IsSelectable)
        {
            throw new DomainException(E.ProductTypeNotSelectable(productType.SlugPath));
        }

        fulfillment.Validate(productType.Kind, shippingProfile);

        var listing = new Listing
        {
            Snapshot = ListingSnapshot.Create(date),
            FulfillmentPreferences = fulfillment,
            Seller = seller,
            ProductType = productType,
            ShippingProfile = shippingProfile,
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

        seller.Listings.Add(listing);
        return listing;
    }

    /// <summary>
    /// Configures the listing's attributes and variants based on the provided attribute inputs and variant axis constraints.
    /// </summary>
    /// <remarks>
    /// All required attributes defined in the product type schema must be present in the inputs. The current set of attributes and variants
    /// for the listing are replaced. This operation can only be performed when the listing is in the draft state.
    /// </remarks>
    /// <param name="snapshotId">The current snapshot ID of the listing. Must match the listing's snapshot ID to ensure consistency.</param>
    /// <param name="inputs">A dictionary containing attribute keys and their corresponding input values. Each key must exist in the product
    /// type schema, and each value must be non-null.</param>
    /// <param name="variantAxesCap">The maximum number of variant axes allowed for the listing. Must be greater than or equal to 1.</param>
    /// <param name="skuCap">The maximum number of SKUs (variants) allowed for the listing. Must be greater than or equal to 1.</param>
    /// <param name="date">The date and time when the attributes are being set.</param>
    public void SetAttributes(
        string snapshotId,
        [NotNull] Dictionary<string, AttributeInputDto> inputs,
        int variantAxesCap,
        int skuCap,
        DateTime date)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(variantAxesCap, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(skuCap, 1);

        if (State is not ListingState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        if (snapshotId != Snapshot.SnapshotId)
        {
            throw new DomainException(E.SnapshotMismatch);
        }

        // Replace the snapshot entirely, staying in draft state (v1)
        Snapshot = ListingSnapshot.Create(date);
        var idx = ProductType.Index!.Hydrated;

        // Ensure all keys on provided inputs exist in the schema and have non-null values
        foreach (var (key, value) in inputs)
        {
            if (!idx.DefsByKey.ContainsKey(key))
            {
                throw new DomainException(E.AttrNotDefined(key, ProductType.SlugPath));
            }
            if (value is null)
            {
                throw new DomainException(E.AttrValueCannotBeNull(key));
            }
        }

        // Ensure all required attributes are present in the inputs
        foreach (var (key, value) in idx.DefsByKey)
        {
            if (value.IsRequired && !inputs.ContainsKey(key))
            {
                throw new DomainException(E.AttrReq(key));
            }
        }

        idx.InitializeListingValidationWithInputs(inputs);

        // Build non-variant attributes (and validate requireds)
        var headerAttrs = BuildNonVariantAttributes(
            inputs,
            out var attrsSnapshot);

        // Build variant axes (independent + composite)
        var axes = BuildVariantAxes(
            inputs,
            variantAxesCap,
            skuCap);

        // Generate variants
        var newVariants = ListingVariantsFactory.GenerateVariants(
            this,
            axes,
            out var axesSnaptshot);

        // Replace state
        Snapshot = Snapshot with
        {
            Attributes = attrsSnapshot,
            Axes = axesSnaptshot
        };

        Variants.Clear();
        Variants.AddRange(newVariants);

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
    /// <param name="snapshotId">The current snapshot ID of the listing. Must match the listing's snapshot ID to ensure consistency.</param>
    /// <param name="inputs">A dictionary containing attribute input data for each axis to append. Each key represents an axis identifier,
    /// and the value provides the choices to be added. Cannot be null.</param>
    /// <param name="skuCap">The maximum allowed number of SKUs after appending new variants.</param>
    /// <param name="date">The date and time when the append operation is performed.</param>
    public void AppendVariantAxes(
        string snapshotId,
        [NotNull] Dictionary<string, AttributeInputDto> inputs,
        int skuCap,
        DateTime date)
    {
        if (State is ListingState.Draft)
        {
            throw new DomainException(E.AppendOnlyPostPublish);
        }

        if (snapshotId != Snapshot.SnapshotId)
        {
            throw new DomainException(E.SnapshotMismatch);
        }

        var idx = ProductType.Index!.Hydrated;
        idx.InitializeListingValidationWithInputs(inputs);

        // Ensure all keys on provided inputs exist in the schema and have non-null values
        foreach (var (key, value) in inputs)
        {
            if (!idx.DefsByKey.ContainsKey(key))
            {
                throw new DomainException(E.AttrNotDefined(key, ProductType.SlugPath));
            }
            if (value is null)
            {
                throw new DomainException(E.AttrValueCannotBeNull(key));
            }
        }

        // Load the baseline axes from the trusted snapshot (immutable, canonical, sorted)
        var baseline = Snapshot.ToRuntime(ProductType);
        var baselineAxisByKey = baseline.ToDictionary(a => a.Definition.Key, StringComparer.Ordinal);

        // Parse the incoming delta as axes (validated, canonicalized, sorted)
        var deltaAxes = BuildVariantAxes(
            inputs,
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

        // Produce the final updated axes snapshot (new id/version/time) from the updated baseline
        var allVariants = ListingVariantsFactory.GenerateVariants(this, updated, out var newAxesSnapshot);
        var existingKeys = Variants.Select(v => v.VariantKey).ToHashSet(StringComparer.Ordinal);

        var newVariants = new List<ListingVariant>(newCount);
        foreach (var v in allVariants)
        {
            if (!existingKeys.Contains(v.VariantKey))
            {
                newVariants.Add(v);
            }
        }
        if (newVariants.Count != newCount)
        {
            Debug.Fail("Logic error: new variant count does not match expectation.");
            throw new InvalidDomainStateException(this, $"Logic error: new variant count '{newVariants.Count}' does not match expected count '{newCount}'.");
        }

        // Update state
        UpdatedAt = date;
        Snapshot = Snapshot.Update(newAxesSnapshot, date);
        // Append new variants
        Variants.AddRange(newVariants);
        // Repoint existing variants' selections to the new snapshot
        foreach (var v in Variants)
        {
            v.SetSelectionSnapshotId(Snapshot);
        }

        // This should always pass, caller should handle InvalidDomainStateException anyway and not commit transaction
        Validate(new ValidationContext(this));
    }

    private List<ListingAttribute> BuildNonVariantAttributes(
        Dictionary<string, AttributeInputDto> inputs,
        out List<HeaderAttrSnapshot> attrsSnapshot)
    {
        attrsSnapshot = [];
        var defsByKey = ProductType.Index!.Hydrated.DefsByKey;
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
            var attr = ListingAttribute.Create(this, def, input);
            result.Add(attr);
            attrsSnapshot.Add(new HeaderAttrSnapshot(def.Key, def.Kind, attr.EnumAttributeOption?.Code ?? attr.LookupOption?.Code ?? attr.Value));
        }

        return result;
    }

    private List<VariantAxis> BuildVariantAxes(
        Dictionary<string, AttributeInputDto> inputs,
        int variantAxesCap,
        int skuCap)
    {
        var idx = ProductType.Index!.Hydrated;
        var axes = new List<VariantAxis>();

        foreach (var (key, input) in inputs)
        {
            if (!idx.DefsByKey.TryGetValue(key, out var def))
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

                        var available = idx.EnumByCode[def.Key];
                        var picked = new List<AxisChoice>(enumOptAxis.Value.Count);

                        foreach (var code in enumOptAxis.Value)
                        {
                            if (!available.TryGetValue(code, out var opt))
                            {
                                throw new DomainException(E.UnknownEnumAttrOpt(def.Key, code));
                            }

                            if (!idx.IsChildCodeReachableFromParents(def.Key, code))
                            {
                                throw new DomainException(E.EnumOptNotReachableFromParents(def.Key, code));
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

                        var available = idx.LookupByCode[def.Key];
                        var picked = new List<AxisChoice>(lookupOptAxis.Value.Count);

                        foreach (var code in lookupOptAxis.Value)
                        {
                            if (!available.TryGetValue(code, out var opt))
                            {
                                throw new DomainException(E.UnknownLookupAttrOpt(def.Key, code));
                            }

                            if (!idx.IsLookupOptionAllowed(def.Key, opt.Code, noEntriesMeansAllowAll: true))
                            {
                                Debug.Assert(!lookupDef.IsOptionAllowed(opt, noEntriesMeansAllowAll: true));
                                throw new DomainException(E.LookupOptNotAllowedByAttr(def.Key, code));
                            }

                            if (!idx.IsChildCodeReachableFromParents(def.Key, code))
                            {
                                throw new DomainException(E.LookupOptNotReachableFromParents(def.Key, code));
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

        prefs.Validate(ProductType.Kind, ShippingProfile);
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

        if (Variants.Find(v => v.SkuCode == sku) is not { } variant)
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
    //    if (Variants.Find(v => v.SkuCode == sku) is not { } variant)
    //    {
    //        throw new DomainException(E.VariantNotFound(sku));
    //    }
    //    variant.Activate();
    //    UpdatedAt = DateTime.UtcNow;
    //}

    //public void DeactivateVariant(string sku)
    //{
    //    if (Variants.Find(v => v.SkuCode == sku) is not { } variant)
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
        if (Variants.Find(v => v.SkuCode == sku) is not { } variant)
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
        if (Variants.Find(v => v.SkuCode == sku) is not { } variant)
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
        FulfillmentPreferences.Validate(ProductType.Kind, ShippingProfile);

        if (ProductType.Kind is ProductTypeKind.Physical)
        {
            foreach (var variant in Variants)
            {
                if (variant.Logistics is null)
                {
                    throw new DomainException(E.LogisticsRequiredForPhysicalProducts);
                }

                variant.Logistics.Validate();
            }
        }

        Validate(new ValidationContext(this));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"L:{Id} - {Title} ({State} | {Variants.Count} axes)";
}
