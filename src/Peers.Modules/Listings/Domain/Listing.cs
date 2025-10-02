using NetTopologySuite.Geometries;
using Peers.Core.Domain.Errors;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Listings.Domain.Translations;
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
    /// The identifier of the seller who created the listing.
    /// </summary>
    public int SellerId { get; private set; }
    /// <summary>
    /// The identifier of the product type associated with this listing.
    /// </summary>
    public int ProductTypeId { get; private set; }
    /// <summary>
    /// The version of the product type at the time the listing was created.
    /// </summary>
    public int ProductTypeVersion { get; private set; }
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
    /// Sets the selected attribute values and variant axes for the listing, updating its attributes and variants
    /// accordingly.
    /// </summary>
    /// <remarks>This method validates the provided attributes against the product type schema and enforces
    /// constraints on required attributes and variant axes. Existing attributes and variants are replaced with the new
    /// configuration.</remarks>
    /// <param name="selectedAttrs">A dictionary mapping attribute keys to their selected values. Each key must correspond to a defined attribute
    /// for the product type. For non-variant attributes, the array must contain exactly one value. For variant axes,
    /// the array must contain one or more option keys.</param>
    /// <param name="maxVariantAxes">The maximum number of variant axes allowed.</param>
    public void SetAttributes(
        [NotNull] Dictionary<string, string[]> selectedAttrs,
        int maxVariantAxes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxVariantAxes, 1);

        if (State is not ListingState.Draft)
        {
            throw new DomainException(E.NotDraft);
        }

        var newAttrs = new List<ListingAttribute>(selectedAttrs.Count);

        var defs = ProductType
            .Attributes
            .ToDictionary(p => p.Key);

        // Ensure all keys on provided values exist in the schema
        foreach (var key in selectedAttrs.Keys)
        {
            if (!defs.ContainsKey(key))
            {
                throw new DomainException(E.AttrNotDefined(key, ProductType.SlugPath));
            }
        }

        var axes = new Dictionary<EnumAttributeDefinition, List<EnumAttributeOption>>();

        foreach (var (_, def) in defs)
        {
            if (selectedAttrs.TryGetValue(def.Key, out var values))
            {
                values ??= [];

                if (def is EnumAttributeDefinition enumDef && enumDef.IsVariant)
                {
                    axes.Add(enumDef, []);
                    if (axes.Count > maxVariantAxes)
                    {
                        throw new DomainException(E.TooManyVariantAxes(maxVariantAxes));
                    }

                    continue;
                }

                // Non-variant path

                if (values.Length != 1)
                {
                    throw new DomainException(E.NonVariantAttrReqExactlyOneValue(def.Key));
                }

                newAttrs.Add(ListingAttribute.Create(this, def, values[0]));
            }
            else if (def.IsRequired)
            {
                throw new DomainException(E.AttrRequired(def.Key));
            }
        }

        // Generate variants from variant enum axes

        foreach (var (axisDef, axisOpts) in axes)
        {
            if (!selectedAttrs.TryGetValue(axisDef.Key, out var selectedOptKeys) || selectedOptKeys.Length == 0)
            {
                throw new DomainException(E.VariantAttrReqAtleastOneOption(axisDef.Key));
            }

            var allowed = axisDef.Options.ToDictionary(o => o.Key);
            var chosen = new List<EnumAttributeOption>(selectedOptKeys.Length);

            foreach (var optKey in selectedOptKeys.Distinct())
            {
                if (!allowed.TryGetValue(optKey, out var opt))
                {
                    throw new DomainException(E.UnknownAttrOption(axisDef.Key, optKey));
                }

                chosen.Add(opt);
            }

            axisOpts.AddRange(chosen);
        }

        var newVariants = axes.Count == 0
            ? [ListingVariant.CreateDefault(this)]
            : ListingVariantsFactory.GenerateVariants(this, axes);

        Variants.Clear();
        Variants.AddRange(newVariants);
        Attributes.Clear();
        Attributes.AddRange(newAttrs);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
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
                throw new DomainException(E.AttrRequired(reqAttr.Key));
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
                .Select(a => a.EnumAttributeOption.Key)
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

                parts.Add(choice.EnumAttributeOption.Key);
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
