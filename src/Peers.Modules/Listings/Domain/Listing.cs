using Peers.Core.Domain.Errors;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Customers.Domain;
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
    /// The order quantity policy for the listing, defining minimum and maximum order quantities.
    /// </summary>
    public OrderQtyPolicy OrderQty { get; private set; }
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
    /// <param name="minOrderQty">The minimum order quantity allowed for the listing, or null if there is no minimum.</param>
    /// <param name="maxOrderQty">The maximum order quantity allowed for the listing, or null if there is no maximum.</param>
    /// <param name="date">The creation date and time for the listing.</param>
    public static Listing Create(
        [NotNull] Customer seller,
        [NotNull] ProductType productType,
        [NotNull] string title,
        string? description,
        string? hashtag,
        decimal price,
        int? minOrderQty,
        int? maxOrderQty,
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

        return new()
        {
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
            OrderQty = OrderQtyPolicy.Create(minOrderQty, maxOrderQty),
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

    //public void Publish()
    //{
    //    if (State == ListingState.Published)
    //    {
    //        return;
    //    }

    //    if (Kind == ListingKind.Physical && ShippingPreferences.Fulfillment == FulfillmentMethod.InsideApp)
    //    {
    //        if (Logistics is null)
    //        {
    //            throw new DomainException("Logistics required to publish.");
    //        }

    //        if (ShippingDecision is null)
    //        {
    //            throw new DomainException("Evaluate shipping before publish.");
    //        }
    //    }

    //    State = ListingState.Published;
    //    UpdatedAt = DateTime.UtcNow;
    //}
}
