using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Listings.Domain.Logistics;

namespace Peers.Modules.Listings.DbMap;

internal sealed class ListingMapping : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        var hashTagColName = nameof(Listing.Hashtag).Underscore();

        builder.HasIndex(p => new { p.SellerId, p.Hashtag })
            .HasFilter($"[{hashTagColName}] IS NOT NULL")
            .IsUnique();

        builder.Property(p => p.Hashtag).HasMaxLength(64);
        builder.Property(p => p.Title).HasMaxLength(256);

        // Concurrency token
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.ComplexProperty(p => p.FulfillmentPreferences, nav =>
        {
            nav.Property(p => p.Method).HasColumnName($"fp_{nameof(FulfillmentPreferences.Method).Underscore()}");
            nav.Property(p => p.OutboundPaidBy).HasColumnName($"fp_{nameof(FulfillmentPreferences.OutboundPaidBy)}".Underscore());
            nav.Property(p => p.ReturnPaidBy).HasColumnName($"fp_{nameof(FulfillmentPreferences.ReturnPaidBy)}".Underscore());
            nav.Property(p => p.NonReturnable).HasColumnName($"fp_{nameof(FulfillmentPreferences.NonReturnable)}".Underscore());
            nav.Property(p => p.OriginLocation).HasColumnName($"fp_{nameof(FulfillmentPreferences.OriginLocation)}".Underscore());
            nav.ComplexProperty(p => p.FreeShippingPolicy, ob =>
            {
                ob.Property(p => p.MinOrder).HasColumnName($"fp_fsp_{nameof(FreeShippingPolicy.MinOrder)}".Underscore());
                ob.Property(p => p.MaxDistance).HasColumnName($"fp_fsp_{nameof(FreeShippingPolicy.MaxDistance)}".Underscore());
            });
            nav.ComplexProperty(p => p.SellerRate, ob =>
            {
                ob.Property(p => p.Kind).HasColumnName($"fp_sr_{nameof(SellerManagedRate.Kind)}".Underscore());
                ob.Property(p => p.FlatAmount).HasColumnName($"fp_sr_{nameof(SellerManagedRate.FlatAmount)}".Underscore());
                ob.Property(p => p.BaseFee).HasColumnName($"fp_sr_{nameof(SellerManagedRate.BaseFee)}".Underscore());
                ob.Property(p => p.RatePerKg).HasColumnName($"fp_sr_{nameof(SellerManagedRate.RatePerKg)}".Underscore());
                ob.Property(p => p.RatePerKm).HasColumnName($"fp_sr_{nameof(SellerManagedRate.RatePerKm)}".Underscore());
                ob.Property(p => p.MinFee).HasColumnName($"fp_sr_{nameof(SellerManagedRate.MinFee)}".Underscore());
            });
            nav.ComplexProperty(p => p.OrderQtyPolicy, ob =>
            {
                ob.Property(p => p.Min).HasColumnName($"fp_oqp_{nameof(OrderQtyPolicy.Min)}".Underscore());
                ob.Property(p => p.Max).HasColumnName($"fp_oqp_{nameof(OrderQtyPolicy.Max)}".Underscore());
            });
            nav.ComplexProperty(p => p.ServiceArea, nav =>
            {
                nav.Property(p => p.Center).HasColumnName($"fp_sa_{nameof(ServiceArea.Center)}".Underscore());
                nav.Property(p => p.Radius).HasColumnName($"fp_sa_{nameof(ServiceArea.Radius)}".Underscore());
            });
        });

        builder.OwnsMany(p => p.Attributes, nav =>
        {
            // one value per listing per def
            nav.HasKey(p => new { p.ListingId, p.AttributeDefinitionId });
            // Per-listing uniqueness for chosen option
            nav.HasIndex(p => new { p.ListingId, p.EnumAttributeOptionId }).IsUnique();
            // Per-listing uniqueness for chosen lookup value
            nav.HasIndex(p => new { p.ListingId, p.LookupValueId }).IsUnique();

            nav.WithOwner(p => p.Listing)
               .HasForeignKey(p => p.ListingId);

            nav.HasOne(p => p.AttributeDefinition)
               .WithMany()
               .HasForeignKey(p => p.AttributeDefinitionId)
               .OnDelete(DeleteBehavior.Restrict);

            nav.HasOne(p => p.EnumAttributeOption)
               .WithMany()
               .HasForeignKey(p => p.EnumAttributeOptionId)
               .OnDelete(DeleteBehavior.Restrict);

            nav.HasOne(p => p.LookupValue)
               .WithMany()
               .HasForeignKey(p => p.LookupValueId)
               .OnDelete(DeleteBehavior.Restrict);

            nav.ToTable(nameof(ListingAttribute).Underscore(), p =>
            {
                var positionPropName = nameof(ListingAttribute.Position);
                var positionColName = positionPropName.Underscore();
                var attrKindColName = nameof(ListingAttribute.AttributeKind).Underscore();
                var valueColName = nameof(ListingAttribute.Value).Underscore();
                var enumOptionIdColName = nameof(ListingAttribute.EnumAttributeOptionId).Underscore();
                var lookupValueIdColName = nameof(ListingAttribute.LookupValueId).Underscore();
                var primitiveAttrKinds = string.Join(',',
                [
                    (int)AttributeKind.Int,
                    (int)AttributeKind.Decimal,
                    (int)AttributeKind.String,
                    (int)AttributeKind.Bool,
                    (int)AttributeKind.Date,
                ]);

                p.HasCheckConstraint($"CK_LA_{positionPropName}_NonNegative", $"[{positionColName}] >= 0");
                p.HasCheckConstraint("CK_LA_OnePayload",
                    $"""
                    (
                        [{attrKindColName}] IN ({primitiveAttrKinds}) AND [{valueColName}] IS NOT NULL
                        AND [{enumOptionIdColName}] IS NULL AND [{lookupValueIdColName}] IS NULL
                    )
                    OR
                    (
                        [{attrKindColName}] = {(int)AttributeKind.Enum} AND [{enumOptionIdColName}] IS NOT NULL
                        AND [{valueColName}] IS NULL AND [{lookupValueIdColName}] IS NULL
                    )
                    OR
                    (
                        [{attrKindColName}] = {(int)AttributeKind.Lookup} AND [{lookupValueIdColName}] IS NOT NULL
                        AND [{valueColName}] IS NULL AND [{enumOptionIdColName}] IS NULL
                    )
                    """);
            });
        });

        builder
            .HasOne(p => p.Seller)
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.ProductType)
            .WithMany()
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(nameof(Listing).Underscore(), p =>
        {
            var basePricePropName = nameof(Listing.BasePrice);
            var basePriceColName = basePricePropName.Underscore();
            var orderQtyPropName = nameof(FulfillmentPreferences.OrderQtyPolicy);
            var minOrderQtyColName = $"fp_oqp_{nameof(OrderQtyPolicy.Min).Underscore()}";
            var maxOrderQtyColName = $"fp_oqp_{nameof(OrderQtyPolicy.Max).Underscore()}";

            p.HasCheckConstraint($"CK_Listing_{basePricePropName}_NonNegative", $"[{basePriceColName}] >= 0");
            p.HasCheckConstraint($"CK_Listing_{orderQtyPropName}",
                $"""
                ([{minOrderQtyColName}] IS NULL OR [{minOrderQtyColName}] >= 1)
                AND ([{maxOrderQtyColName}] IS NULL OR [{maxOrderQtyColName}] >= 1)
                AND ([{minOrderQtyColName}] IS NULL OR [{maxOrderQtyColName}] IS NULL OR [{maxOrderQtyColName}] >= [{minOrderQtyColName}])
                """
            );
        });

        builder.Navigation(p => p.Attributes).AutoInclude(false);
    }
}
