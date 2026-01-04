//using System.Globalization;
//using Microsoft.EntityFrameworkCore;
//using Peers.Core.Nafath.Models;
//using Peers.Modules.Catalog.Domain;
//using Peers.Modules.Catalog.Domain.Attributes;
//using Peers.Modules.Listings.Domain;
//using Peers.Modules.Listings.Domain.Logistics;
//using Peers.Modules.Lookup.Domain;
//using Peers.Modules.Sellers.Domain;
//using Peers.Modules.Users.Domain;
//using static Peers.Modules.Listings.Commands.SetAttributes.Command;
//using static Peers.Modules.Listings.Commands.SetAttributes.Command.AttributeInputDto;

//namespace Peers.Modules.Test.Catalog;

//public class VariantsGenTests
//{
//    [Fact]
//    public void DoIt()
//    {
//        var nafathIdentity = new NafathIdentity("1111111111", null, null, null, null, null);
//        var seller = Seller.Create(AppUser.CreateTwoFactorAccount(DateTime.UtcNow, "Dapp", "+966500000000", "en"), nafathIdentity);

//        var colorLookup = CreateLookupWithOptions("g-color", LookupConstraintMode.Open, true,
//            [
//                "g-black",
//                "g-white",
//                "g-gray",
//                "g-silver",
//                "g-blue",
//                "g-navy",
//                "g-red",
//                "g-green",
//                "g-yellow",
//                "g-orange",
//                "g-pink",
//                "g-purple",
//                "g-brown",
//                "g-beige",
//                "g-gold",
//                "g-multicolor",
//            ]);

//        var babyProductsBrandLookup = CreateLookupWithOptions("g-baby_products_brand", LookupConstraintMode.Open, false,
//            [
//                "g-pampers",
//                "g-huggies",
//                "g-littleswimmers",
//                "g-honest",
//            ]);

//        var babyProductsProductType = ProductType.CreateRoot(ProductTypeKind.Physical, "Baby Products");
//        babyProductsProductType.DefineLookupAttribute("brand", true, false, 10, babyProductsBrandLookup);
//        babyProductsProductType.Publish();

//        var diapersProductType = babyProductsProductType.AddChild("Diapers", true, true);
//        diapersProductType.DefineEnumAttribute("size", true, false, 20);
//        Enumerable.Range(1, 6).ToList().ForEach(n => diapersProductType.AddAttributeOption("size", n.ToString(CultureInfo.InvariantCulture), n));
//        diapersProductType.DefineAttribute("unit_count", AttributeKind.Int, true, true, 30, unit: "diaper");
//        diapersProductType.Publish();

//        var diaperListing = Listing.Create(
//            seller: seller,
//            productType: diapersProductType,
//            fulfillment: new FulfillmentPreferences(FulfillmentMethod.PlatformManaged, null, null, null, null, null),
//            title: "Pampers Diapers",
//            description: "Comfortable and absorbent diapers for your baby.",
//            hashtag: "pampers-diapers",
//            price: 29.99m,
//            date: DateTime.UtcNow,
//            shippingProfile: null);
//        diaperListing.Id = 1;
//        diaperListing.SetAttributes(diaperListing.Snapshot.SnapshotId, new Dictionary<string, AttributeInputDto>
//        {
//            ["brand"] = new OptionCodeOrScalarString("pampers"),
//            ["size"] = new OptionCodeOrScalarString("3"),
//            ["unit_count"] = new NumericAxis([50, 100, 32, 150, 7]),
//        }, 3, 100, DateTime.UtcNow);

//        // Set fulfillment/logistics
//        diaperListing.SetFulfillmentPreferences(diaperListing.FulfillmentPreferences with
//        {
//            Method = FulfillmentMethod.PlatformManaged,
//            OutboundPaidBy = ShippingCostPayer.Seller,
//            NonReturnable = true,
//        });
//        foreach (var variant in diaperListing.Variants)
//        {
//            diaperListing.SetLogistics(variant.SkuCode, new LogisticsProfile(new Dimensions(10, 10, 10), 10, false, false, TemperatureControl.None));
//        }
//        //
//        diaperListing.Publish();

//        diaperListing.AppendVariantAxes(diaperListing.Snapshot.SnapshotId, new Dictionary<string, AttributeInputDto>
//        {
//            ["unit_count"] = new NumericAxis([214, 50, 7, 1]),
//        }, 100, DateTime.UtcNow);

//        // ---------------- Dining Tables ----------------

//        var homeAndLivingBrandLookup = CreateLookupWithOptions("home_and_living_brand", LookupConstraintMode.Open, false,
//            [
//                "ikea",
//                "wayfair",
//                "ashley",
//            ]);

//        var pos = 0;
//        var homeAndLivingProductType = ProductType.CreateRoot(ProductTypeKind.Physical, "Home & Living");
//        homeAndLivingProductType.DefineLookupAttribute("brand", true, false, 10, homeAndLivingBrandLookup);
//        homeAndLivingProductType.Publish();

//        var diningTableProductType = homeAndLivingProductType.AddChild("Dining Tables", true, true);
//        diningTableProductType.DefineEnumAttribute("material", false, true, 1000);
//        foreach (var (code, label) in new[] { ("wood", "Wood"), ("metal", "Metal"), ("glass", "Glass"), ("plastic", "Plastic") })
//        {
//            diningTableProductType.AddAttributeOption("material", code, pos++);
//        }
//        diningTableProductType.DefineEnumAttribute("quality", false, false, 900);
//        foreach (var (code, label) in new[] { ("standard", "Standard"), ("premium", "Premium"), ("luxury", "Luxury") })
//        {
//            diningTableProductType.AddAttributeOption("quality", code, pos++);
//        }
//        diningTableProductType.DefineAttribute("size", AttributeKind.Group, false, true, 70);
//        diningTableProductType.DefineEnumAttribute("shape", true, false, 30);
//        diningTableProductType.DefineLookupAttribute("color", true, true, 20, colorLookup);
//        diningTableProductType.DefineEnumAttribute("base_color", false, true, 1200);
//        foreach (var (code, label) in new[] { ("black", "Black"), ("white", "White"), ("gray", "Gray"), ("silver", "Silver"), ("blue", "Blue"), ("navy", "Navy"), ("red", "Red"), ("green", "Green"), ("yellow", "Yellow"), ("orange", "Orange"), ("pink", "Pink"), ("purple", "Purple"), ("brown", "Brown"), ("beige", "Beige"), ("gold", "Gold"), ("multicolor", "Multicolor") })
//        {
//            diningTableProductType.AddAttributeOption("base_color", code, pos++);
//        }
//        foreach (var (code, label) in new[] { ("round", "Round"), ("square", "Square"), ("rectangle", "Rectangle"), ("oval", "Oval") })
//        {
//            diningTableProductType.AddAttributeOption("shape", code, pos++);
//        }
//        diningTableProductType.DefineAttribute("height", AttributeKind.Int, true, false, 60, unit: "cm");
//        diningTableProductType.DefineAttribute("width", AttributeKind.Int, false, false, 50, unit: "cm");
//        diningTableProductType.DefineAttribute("length", AttributeKind.Int, false, false, 40, unit: "cm");
//        diningTableProductType.AddGroupAttributeMember("size", "length");
//        diningTableProductType.AddGroupAttributeMember("size", "width");

//        diningTableProductType.DefineAttribute("g2", AttributeKind.Int, false, false, 5);
//        diningTableProductType.DefineAttribute("g1", AttributeKind.Int, false, false, 1);
//        diningTableProductType.DefineAttribute("g3", AttributeKind.Int, false, false, 101);
//        diningTableProductType.DefineAttribute("group", AttributeKind.Group, false, true, 100);
//        diningTableProductType.AddGroupAttributeMember("group", "g2");
//        diningTableProductType.AddGroupAttributeMember("group", "g3");
//        diningTableProductType.AddGroupAttributeMember("group", "g1");

//        diningTableProductType.Publish();

//        var diningTableListing = Listing.Create(
//            seller: seller,
//            productType: diningTableProductType,
//            fulfillment: new FulfillmentPreferences(FulfillmentMethod.PlatformManaged, null, null, null, null, null),
//            title: "IKEA Dining Table",
//            description: "Stylish and functional dining table for your home.",
//            hashtag: "ikea-dining-table",
//            price: 199.99m,
//            date: DateTime.UtcNow,
//            shippingProfile: null);
//        diningTableListing.Id = 2;
//        diningTableListing.SetAttributes(diningTableListing.Snapshot.SnapshotId, new Dictionary<string, AttributeInputDto>
//        {
//            ["material"] = new OptionCodeAxis(["wood"]),
//            ["group"] = new GroupAxis(
//            [
//                new([10, 20, 30]),
//                new([100, 200, 300]),
//                new([1, 2, 3]),
//            ]),
//            ["brand"] = new OptionCodeOrScalarString("ikea"),
//            ["color"] = new OptionCodeAxis(["pink", "black", "white"]),
//            ["base_color"] = new OptionCodeAxis(["pink"]),
//            ["shape"] = new OptionCodeOrScalarString("rectangle"),
//            ["size"] = new GroupAxis(
//            [
//                new([100, 150]),
//                new([50, 90]),
//                new([150, 200]),
//                new([75, 120]),
//            ]),
//            ["height"] = new Numeric(75),
//        }, 5, 200, DateTime.UtcNow);

//        // Set fulfillment/logistics
//        diningTableListing.SetFulfillmentPreferences(diningTableListing.FulfillmentPreferences with
//        {
//            Method = FulfillmentMethod.PlatformManaged,
//            OutboundPaidBy = ShippingCostPayer.Seller,
//            NonReturnable = true,
//        });
//        foreach (var variant in diningTableListing.Variants)
//        {
//            diningTableListing.SetLogistics(variant.SkuCode, new LogisticsProfile(new Dimensions(10, 10, 10), 10, false, false, TemperatureControl.None));
//        }
//        //
//        diningTableListing.Publish();
//        diningTableListing.AppendVariantAxes(diningTableListing.Snapshot.SnapshotId, new Dictionary<string, AttributeInputDto>
//        {
//            ["size"] = new GroupAxis(
//            [
//                new([300, 300]),
//            ]),
//            ["color"] = new OptionCodeAxis(["blue", "navy", "red", "green"]),
//            ["group"] = new GroupAxis(
//            [
//                new([2000, 2000, 2000]),
//                new([1000, 1000, 1000]),
//            ]),
//        }, 200, DateTime.UtcNow);
//    }

//    private static LookupType CreateLookupWithOptions(string key, LookupConstraintMode constraintMode, bool allowVariants, string[] codes)
//    {
//        var lookupType = new LookupType(key, constraintMode, allowVariants);
//        foreach (var code in codes)
//        {
//            lookupType.CreateOption(code);
//        }

//        return lookupType;
//    }
//}

//[Collection(nameof(IntegrationTestBaseCollection))]
//public class DelMe : IntegrationTestBase
//{
//    [Fact]
//    public async Task CatalogIndexTest()
//    {
//        await LookupLinkRelationTest();
//        await ExecuteDbContextAsync(async db =>
//        {
//            var brandLookup = db.LookupTypes
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//                .FirstOrDefault(lt => lt.Key == "g-brand");
//            var modelLookup = db.LookupTypes
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//                .FirstOrDefault(lt => lt.Key == "g-model");
//            var colorLookup = db.LookupTypes.Include(p => p.Options)
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//                .FirstOrDefault(lt => lt.Key == "g-color");

//            var electronicsProductType = ProductType.CreateRoot(ProductTypeKind.Physical, "Electronics");
//            electronicsProductType.DefineLookupAttribute("brand", true, false, 10, brandLookup);
//            electronicsProductType.DefineLookupAttribute("model", true, false, 20, modelLookup);
//            electronicsProductType.Publish();

//            var mobilePhonesProductType = electronicsProductType.AddChild("Mobile Phones", isSelectable: true, copyAttributes: true);
//            mobilePhonesProductType.DefineLookupAttribute("color", true, true, 30, colorLookup);
//            //mobilePhonesProductType.AddAllowedLookup(colorLookup.Options[0]); // black
//            //mobilePhonesProductType.AddAllowedLookup(colorLookup.Options[1]); // white
//            mobilePhonesProductType.AddAllowedLookup("color", colorLookup.Options.First(p => p.Code == "g-gold"));
//            //mobilePhonesProductType.AddAllowedLookup("model", modelLookup.Options.First(p => p.Code == "iPhone 15"));
//            mobilePhonesProductType.Publish();

//            db.ProductTypes.Add(electronicsProductType);
//            await db.SaveChangesAsync();
//        });

//        await ExecuteDbContextAsync(async db =>
//        {
//            var pt = await db.ProductTypes
//                .AsNoTrackingWithIdentityResolution()
//                .Include(p => p.Index)
//                .Include(p => p.Attributes).ThenInclude(p => ((EnumAttributeDefinition)p).Options)
//                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).AllowedOptions)
//                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(lt => lt.Options)
//                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(t => t.ParentLinks).ThenInclude(l => l.ChildOption)
//                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(t => t.ChildLinks).ThenInclude(l => l.ParentOption)
//                .FirstOrDefaultAsync(p => p.Slug == "mobile-phones");

//            var catalogIndex = pt.Index.Hydrated;
//            var allowed1 = catalogIndex.IsLookupOptionAllowed("color", "purple");
//            var allowed2 = catalogIndex.TryGetLookupAllowedChildren("model", "color", "g-iphone-15", out var allowedColorsForIphone);
//        });
//    }

//    [Fact]
//    public async Task LookupLinkRelationTest()
//    {
//        await ExecuteDbContextAsync(async db =>
//        {
//            var brandLookup = CreateLookupWithOptions("g-brand", LookupConstraintMode.Open, false,
//                [
//                    "g-apple",
//                    "g-samsung",
//                    "g-adidas",
//                ]);

//            db.LookupTypes.Add(brandLookup);
//            await db.SaveChangesAsync();

//            var modelLookup = CreateLookupWithOptions("g-model", LookupConstraintMode.Open, false,
//                [
//                    "g-iphone-15",
//                    "g-iphone-14",
//                    "g-galaxy-s24",
//                    "g-galaxy-note-20",
//                    "g-ultra-boost",
//                    "g-nmd",
//                ]);

//            db.LookupTypes.Add(modelLookup);
//            await db.SaveChangesAsync();

//            var colorLookup = CreateLookupWithOptions("g-color", LookupConstraintMode.RequireAllowList, true,
//                [
//                    "g-black",
//                    "g-white",
//                    "g-gray",
//                    "g-silver",
//                    "g-blue",
//                    "g-navy",
//                    "g-red",
//                    "g-green",
//                    "g-yellow",
//                    "g-orange",
//                    "g-pink",
//                    "g-purple",
//                    "g-brown",
//                    "g-beige",
//                    "g-gold",
//                ]);

//            db.LookupTypes.Add(colorLookup);
//            await db.SaveChangesAsync();
//        });

//        await ExecuteDbContextAsync(async db =>
//        {
//            var brandLookup = await db.LookupTypes.Include(p => p.Options).FirstAsync(lt => lt.Key == "g-brand");
//            var modelLookup = await db.LookupTypes.Include(p => p.Options).FirstAsync(lt => lt.Key == "g-model");
//            var colorLookup = await db.LookupTypes.Include(p => p.Options).FirstAsync(lt => lt.Key == "g-color");

//            // Link Apple model options to Apple brand options
//            var appleBrandOption = brandLookup.Options.First(o => o.Code == "g-apple");
//            var appleModelOptions = modelLookup.Options.Where(o => o.Code.StartsWith("g-iphone")).ToList();
//            db.LookupLinks.Add(new LookupLink
//            {
//                ParentType = brandLookup,
//                ParentOption = appleBrandOption,
//                ChildType = modelLookup,
//                ChildOption = appleModelOptions[0],
//            });
//            db.LookupLinks.Add(new LookupLink
//            {
//                ParentType = brandLookup,
//                ParentOption = appleBrandOption,
//                ChildType = modelLookup,
//                ChildOption = appleModelOptions[1],
//            });

//            // Link Samsung model options to Samsung brand options
//            var samsungBrandOption = brandLookup.Options.First(o => o.Code == "g-samsung");
//            var samsungModelOptions = modelLookup.Options.Where(o => o.Code.StartsWith("g-galaxy")).ToList();
//            db.LookupLinks.Add(new LookupLink
//            {
//                ParentType = brandLookup,
//                ParentOption = samsungBrandOption,
//                ChildType = modelLookup,
//                ChildOption = samsungModelOptions[0],
//            });
//            db.LookupLinks.Add(new LookupLink
//            {
//                ParentType = brandLookup,
//                ParentOption = samsungBrandOption,
//                ChildType = modelLookup,
//                ChildOption = samsungModelOptions[1],
//            });

//            // Link Adidas model options to Adidas brand options
//            var adidasBrandOption = brandLookup.Options.First(o => o.Code == "g-adidas");
//            var adidasModelOptions = modelLookup.Options.Where(o => o.Code == "g-ultra-boost" || o.Code == "g-nmd").ToList();
//            db.LookupLinks.Add(new LookupLink
//            {
//                ParentType = brandLookup,
//                ParentOption = adidasBrandOption,
//                ChildType = modelLookup,
//                ChildOption = adidasModelOptions[0],
//            });
//            db.LookupLinks.Add(new LookupLink
//            {
//                ParentType = brandLookup,
//                ParentOption = adidasBrandOption,
//                ChildType = modelLookup,
//                ChildOption = adidasModelOptions[1],
//            });

//            // iPhones can only be black, white, or gold
//            var iphoneModels = modelLookup.Options.Where(o => o.Code.StartsWith("g-iphone")).ToList();
//            var allowedColorsForIphones = colorLookup.Options.Where(o => o.Code == "g-black" || o.Code == "g-white" || o.Code == "g-gold").ToList();
//            foreach (var modelOption in iphoneModels)
//            {
//                foreach (var colorOption in allowedColorsForIphones)
//                {
//                    db.LookupLinks.Add(new LookupLink
//                    {
//                        ParentType = modelLookup,
//                        ParentOption = modelOption,
//                        ChildType = colorLookup,
//                        ChildOption = colorOption,
//                    });
//                }
//            }

//            // Samsung Galaxy models can only be black, white, blue, or silver
//            var galaxyModels = modelLookup.Options.Where(o => o.Code.StartsWith("g-galaxy")).ToList();
//            var allowedColorsForGalaxy = colorLookup.Options.Where(o => o.Code == "g-black" || o.Code == "g-white" || o.Code == "g-blue" || o.Code == "g-silver").ToList();
//            foreach (var modelOption in galaxyModels)
//            {
//                foreach (var colorOption in allowedColorsForGalaxy)
//                {
//                    db.LookupLinks.Add(new LookupLink
//                    {
//                        ParentType = modelLookup,
//                        ParentOption = modelOption,
//                        ChildType = colorLookup,
//                        ChildOption = colorOption,
//                    });
//                }
//            }

//            await db.SaveChangesAsync();
//        });

//        await ExecuteDbContextAsync(async db =>
//        {
//            // Verify the links
//            var brandLookup = await db.LookupTypes
//                .Include(lt => lt.Options.Where(p => p.Code == "g-apple"))
//                .Include(t => t.ParentLinks).ThenInclude(l => l.ChildOption)
//                .Include(t => t.ChildLinks).ThenInclude(l => l.ParentOption)
//                .FirstAsync(lt => lt.Key == "g-brand");

//            var modelLookup = await db.LookupTypes
//                .Include(lt => lt.Options)
//                .Include(t => t.ParentLinks).ThenInclude(l => l.ChildOption)
//                .Include(t => t.ChildLinks).ThenInclude(l => l.ParentOption)
//                .FirstAsync(lt => lt.Key == "g-model");

//            var links = await db.LookupLinks
//                .Include(l => l.ParentType)
//                .Include(l => l.ChildType)
//                .Include(ll => ll.ParentOption)
//                .Include(ll => ll.ChildOption)
//                .ToListAsync();
//            Assert.Equal(20, links.Count);
//        });
//    }

//    [Fact]
//    public async Task XXX()
//    {
//        await ExecuteDbContextAsync(db => CatalogSeeder.SeedAsync(db));
//        await ExecuteDbContextAsync(db => DemoDataSeeder.SeedAsync(db));
//        await ExecuteDbContextAsync(async db =>
//        {
//            List<PostQueriesForBuyer.ProductSummaryDto?> buyerResults = [];
//            List<object?> sellerResults = [];
//            var ids = await db.Listings.Select(l => l.Id).ToArrayAsync();
//            foreach (var id in ids)
//            {
//                buyerResults.Add(await PostQueriesForBuyer.GetListingDetailsAsync(db, id));
//                sellerResults.Add(await PostQueriesForSeller.GetListingEditorAsync(db, id));
//            }
//        });

//        Console.WriteLine("");
//    }
//    public static class CatalogSeeder
//    {
//        public static async Task SeedAsync(PeersContext db, CancellationToken ct = default)
//        {
//            // Lookup types (idempotent by Key)
//            var countryLookup = CreateLookupWithOptions("g-country", LookupConstraintMode.Open, false,
//                [
//                    "g-china",
//                    "g-south_korea",
//                    "g-united_states",
//                    "g-united_kingdom",
//                ]);
//            var plugTypeLookup = CreateLookupWithOptions("g-plug_type", LookupConstraintMode.Open, false,
//                [
//                    "g-type_a",
//                    "g-type_b",
//                    "g-type_c",
//                    "g-type_d",
//                ]);

//            db.LookupTypes.AddRange(countryLookup, plugTypeLookup);
//            await db.SaveChangesAsync(ct);

//            countryLookup = db.LookupTypes
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//                .FirstOrDefault(lt => lt.Key == "g-country");
//            plugTypeLookup = db.LookupTypes
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//                .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//                .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//                .FirstOrDefault(lt => lt.Key == "g-plug_type");

//            // Keep it transactional & idempotent
//            await using var tx = await db.Database.BeginTransactionAsync(ct);

//            // ROOTS
//            var electronics = ProductType.CreateRoot(ProductTypeKind.Physical, "Electronics");
//            var fashion = ProductType.CreateRoot(ProductTypeKind.Physical, "Fashion");
//            var home = ProductType.CreateRoot(ProductTypeKind.Physical, "Home & Living");
//            var services = ProductType.CreateRoot(ProductTypeKind.Service, "Services");

//            electronics.DefineLookupAttribute("made_in", true, false, 5, countryLookup);
//            electronics.AddAllowedLookup("made_in", countryLookup.Options[0]);
//            electronics.AddAllowedLookup("made_in", countryLookup.Options[1]);
//            electronics.AddAllowedLookup("made_in", countryLookup.Options[2]);
//            electronics.AddAllowedLookup("made_in", countryLookup.Options[3]);

//            electronics.DefineLookupAttribute("plug_type", false, false, 6, plugTypeLookup);
//            electronics.AddAllowedLookup("plug_type", plugTypeLookup.Options[0]);
//            electronics.AddAllowedLookup("plug_type", plugTypeLookup.Options[1]);
//            electronics.AddAllowedLookup("plug_type", plugTypeLookup.Options[2]);

//            fashion.DefineLookupAttribute("made_in", true, false, 5, countryLookup);
//            fashion.AddAllowedLookup("made_in", countryLookup.Options[0]);
//            fashion.AddAllowedLookup("made_in", countryLookup.Options[1]);
//            fashion.AddAllowedLookup("made_in", countryLookup.Options[2]);
//            fashion.AddAllowedLookup("made_in", countryLookup.Options[3]);

//            // --- Root-level shared attributes (inherited) ---
//            ConfigureElectronicsRoot(electronics);  // e.g., brand, color
//            ConfigureFashionRoot(fashion);          // e.g., brand, color, material

//            electronics.Publish();
//            fashion.Publish();
//            home.Publish();
//            services.Publish();

//            // LEAVES (selectable)
//            var phones = electronics.AddChild("Mobile Phones", isSelectable: true, copyAttributes: true);
//            var laptops = electronics.AddChild("Laptops", isSelectable: true, copyAttributes: true);
//            ConfigureElectronicsPhones(phones);     // storage_gb, ram_gb, screen_size_in, battery_mAh, warranty_months
//            ConfigureElectronicsLaptops(laptops);   // storage_gb, ram_gb, screen_size_in, etc.

//            phones.RemoveAllowedLookup("made_in", countryLookup.Options[2]);
//            phones.RemoveAllowedLookup("made_in", countryLookup.Options[3]);

//            phones.Publish();
//            laptops.Publish();

//            var footwear = fashion.AddChild("Footwear", isSelectable: true, copyAttributes: true);
//            var apparel = fashion.AddChild("Apparel", isSelectable: true, copyAttributes: true);
//            ConfigureFootwearLeaf(footwear);
//            ConfigureApparelLeaf(apparel);

//            footwear.Publish();
//            apparel.Publish();

//            var furniture = home.AddChild("Furniture", isSelectable: true, copyAttributes: true);
//            ConfigureFurnitureLeaf(furniture);

//            furniture.Publish();

//            var serviceBasic = services.AddChild("Service Basic", isSelectable: true, copyAttributes: true);
//            ConfigureServiceBasicLeaf(serviceBasic);

//            serviceBasic.Publish();

//            db.ProductTypes.AddRange([electronics, fashion, home, services]);
//            await db.SaveChangesAsync(ct);
//            await tx.CommitAsync(ct);
//        }

//        // --- Root configs (inherited) ---
//        private static void ConfigureElectronicsRoot(ProductType root)
//        {
//            var brand = root.DefineAttribute("brand", AttributeKind.Enum, true, false, 10);
//            var i = 0;
//            var brandSamsungOption = root.AddAttributeOption(brand.Key, "samsung", i++);
//            root.AddAttributeOption(brand.Key, "sony", i++);
//            root.AddAttributeOption(brand.Key, "lg", i++);
//            root.AddAttributeOption(brand.Key, "other", i++);

//            var samsungKnoxMode = root.DefineDependentAttribute(brand.Key, "samsung_knox", false, false, 11);
//            i = 0;
//            root.AddAttributeOption(samsungKnoxMode.Key, "standard", i++, brandSamsungOption.Code);
//            root.AddAttributeOption(samsungKnoxMode.Key, "secure", i++, brandSamsungOption.Code);

//            var color = root.DefineAttribute("color", AttributeKind.Enum, false, true, 20);
//            SeedColors(root, color);
//        }

//        private static void ConfigureFashionRoot(ProductType root)
//        {
//            var brand = root.DefineAttribute("brand", AttributeKind.Enum, true, false, 10);
//            var i = 0;
//            root.AddAttributeOption(brand.Key, "nike", i++);
//            root.AddAttributeOption(brand.Key, "adidas", i++);
//            root.AddAttributeOption(brand.Key, "tommy_hilfiger", i++);
//            root.AddAttributeOption(brand.Key, "other", i++);

//            var color = root.DefineAttribute("color", AttributeKind.Enum, true, true, 20);
//            SeedColors(root, color);

//            var material = root.DefineAttribute("material", AttributeKind.Enum, false, false, 30);
//            SeedMaterials(root, material);
//        }

//        // --- Leaf configs (additive) ---
//        private static void ConfigureElectronicsPhones(ProductType leaf)
//        {
//            leaf.DefineAttribute("model", AttributeKind.String, true, false, 30);
//            leaf.DefineAttribute("storage_gb", AttributeKind.Int, false, false, 40);
//            leaf.DefineAttribute("screen_size_in", AttributeKind.Decimal, false, false, 60, unit: "in");
//        }

//        private static void ConfigureElectronicsLaptops(ProductType leaf)
//        {
//            leaf.DefineAttribute("model", AttributeKind.String, true, false, 30);
//            leaf.DefineAttribute("storage_gb", AttributeKind.Int, false, false, 40);
//            leaf.DefineAttribute("ram_gb", AttributeKind.Int, false, false, 50);
//            leaf.DefineAttribute("screen_size_in", AttributeKind.Decimal, false, false, 60, unit: "in");
//        }

//        private static void ConfigureFootwearLeaf(ProductType leaf)
//        {
//            leaf.DefineAttribute("subtype", AttributeKind.Enum, true, false, 25);
//            leaf.DefineAttribute("upper_material", AttributeKind.Enum, false, false, 35);
//            leaf.DefineAttribute("sole_material", AttributeKind.Enum, false, false, 45);
//            leaf.DefineAttribute("size_value_eu", AttributeKind.Enum, true, true, 110, unit: "eu");
//            // options
//            SeedFootwearSubtype(leaf, rootOr(leaf, "subtype"));
//            SeedUpperMaterials(leaf, rootOr(leaf, "upper_material"));
//            SeedSoleMaterials(leaf, rootOr(leaf, "sole_material"));
//            SeedFootwearSizes(leaf, rootOr(leaf, "size_value_eu"));

//            AttributeDefinition rootOr(ProductType t, string key) => t.Attributes.First(a => a.Key == key);
//        }

//        private static void ConfigureApparelLeaf(ProductType leaf)
//        {
//            leaf.DefineAttribute("gender", AttributeKind.Enum, true, false, 25);
//            leaf.DefineAttribute("pattern", AttributeKind.Enum, false, false, 35);
//            leaf.DefineAttribute("size_value", AttributeKind.Enum, true, true, 110);
//            SeedGender(leaf, rootOr(leaf, "gender"));
//            SeedPatterns(leaf, rootOr(leaf, "pattern"));
//            SeedApparelSizes(leaf, rootOr(leaf, "size_value"));

//            AttributeDefinition rootOr(ProductType t, string key) => t.Attributes.First(a => a.Key == key);
//        }

//        private static void ConfigureFurnitureLeaf(ProductType leaf)
//        {
//            leaf.DefineAttribute("length_cm", AttributeKind.Decimal, false, false, 30, unit: "cm");
//            leaf.DefineAttribute("width_cm", AttributeKind.Decimal, false, false, 40, unit: "cm");
//            leaf.DefineAttribute("height_cm", AttributeKind.Decimal, false, false, 50, unit: "cm");
//            leaf.DefineAttribute("weight_kg", AttributeKind.Decimal, false, false, 60, unit: "kg");
//            leaf.DefineAttribute("assembly_required", AttributeKind.Bool, false, false, 70);
//        }

//        private static void ConfigureServiceBasicLeaf(ProductType leaf)
//        {
//            leaf.DefineAttribute("service_category", AttributeKind.Enum, true, false, 10);
//            leaf.DefineAttribute("location_kind", AttributeKind.Enum, true, false, 20);
//            leaf.DefineAttribute("service_area", AttributeKind.String, false, false, 30);
//            leaf.DefineAttribute("duration_min", AttributeKind.Int, false, false, 40);
//            leaf.DefineAttribute("capacity", AttributeKind.Int, false, false, 50);
//            leaf.DefineAttribute("lead_time_hours", AttributeKind.Int, false, false, 60);
//            leaf.DefineAttribute("cancellation_policy", AttributeKind.Enum, false, false, 70);

//            SeedServiceCategories(leaf, rootOr(leaf, "service_category"));
//            SeedLocationKinds(leaf, rootOr(leaf, "location_kind"));
//            SeedCancellationPolicies(leaf, rootOr(leaf, "cancellation_policy"));

//            AttributeDefinition rootOr(ProductType t, string key) => t.Attributes.First(a => a.Key == key);
//        }

//        // -------------------- Option libraries --------------------

//        private static void SeedColors(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            foreach (var (code, label) in new[]
//            {
//            ("black","Black"), ("white","White"), ("gray","Gray"), ("silver","Silver"),
//            ("blue","Blue"), ("navy","Navy"), ("red","Red"), ("green","Green"),
//            ("yellow","Yellow"), ("orange","Orange"), ("pink","Pink"),
//            ("purple","Purple"), ("brown","Brown"), ("beige","Beige"),
//            ("gold","Gold"), ("multicolor","Multicolor")
//        })
//            {
//                pt.AddAttributeOption(def.Key, code, i++);
//            }
//        }

//        private static void SeedMaterials(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            foreach (var (code, label) in new[]
//            {
//            ("cotton","Cotton"), ("polyester","Polyester"), ("wool","Wool"),
//            ("linen","Linen"), ("leather","Leather"), ("suede","Suede"),
//            ("mesh","Mesh"), ("canvas","Canvas"), ("rubber","Rubber"),
//            ("plastic","Plastic"), ("metal","Metal"), ("wood","Wood"),
//            ("glass","Glass"), ("ceramic","Ceramic"), ("other","Other")
//        })
//            {
//                pt.AddAttributeOption(def.Key, code, i++);
//            }
//        }

//        private static void SeedGender(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            pt.AddAttributeOption(def.Key, "men", i++);
//            pt.AddAttributeOption(def.Key, "women", i++);
//            pt.AddAttributeOption(def.Key, "unisex", i++);
//            pt.AddAttributeOption(def.Key, "kids", i++);
//        }

//        private static void SeedPatterns(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            foreach (var (code, label) in new[]
//            {
//            ("solid","Solid"), ("striped","Striped"), ("plaid","Plaid"),
//            ("printed","Printed"), ("logo","Logo")
//        })
//            {
//                pt.AddAttributeOption(def.Key, code, i++);
//            }
//        }

//        private static void SeedApparelSizes(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            foreach (var (code, label) in new[]
//            {
//            ("xs","XS"), ("s","S"), ("m","M"), ("l","L"), ("xl","XL"),
//            ("xxl","XXL"), ("3xl","XXXL"), ("onesize","One Size")
//        })
//            {
//                pt.AddAttributeOption(def.Key, code, i++);
//            }
//        }

//        private static void SeedFootwearSizes(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            Enumerable.Range(35, 10).ToList().ForEach(n => pt.AddAttributeOption(def.Key, n.ToString(CultureInfo.InvariantCulture), i++));
//        }

//        private static void SeedFootwearSubtype(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            foreach (var (code, label) in new[]
//            {
//            ("sneaker","Sneaker"), ("running","Running"), ("hiking","Hiking"),
//            ("boot","Boot"), ("sandal","Sandal"), ("heel","Heel"),
//            ("loafer","Loafer"), ("oxford","Oxford"), ("football","Football"),
//            ("basketball","Basketball"), ("flipflop","Flip Flop"), ("other","Other")
//        })
//            {
//                pt.AddAttributeOption(def.Key, code, i++);
//            }
//        }

//        private static void SeedUpperMaterials(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            foreach (var (code, label) in new[]
//            {
//            ("leather","Leather"), ("synthetic","Synthetic"), ("mesh","Mesh"),
//            ("suede","Suede"), ("canvas","Canvas"), ("knit","Knit"), ("other","Other")
//        })
//            {
//                pt.AddAttributeOption(def.Key, code, i++);
//            }
//        }

//        private static void SeedSoleMaterials(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            foreach (var (code, label) in new[]
//            {
//            ("rubber","Rubber"), ("eva","EVA"), ("tpu","TPU"), ("pu","PU"), ("leather","Leather"), ("other","Other")
//        })
//            {
//                pt.AddAttributeOption(def.Key, code, i++);
//            }
//        }

//        private static void SeedServiceCategories(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            foreach (var (code, label) in new[]
//            {
//            ("cleaning","Cleaning"), ("repair","Repair"), ("consulting","Consulting"),
//            ("tutoring","Tutoring"), ("trip","Trip"), ("delivery","Delivery")
//        })
//            {
//                pt.AddAttributeOption(def.Key, code, i++);
//            }
//        }

//        private static void SeedLocationKinds(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            pt.AddAttributeOption(def.Key, "on_site", i++);
//            pt.AddAttributeOption(def.Key, "provider", i++);
//            pt.AddAttributeOption(def.Key, "remote", i++);
//        }

//        private static void SeedCancellationPolicies(ProductType pt, AttributeDefinition def)
//        {
//            var i = 0;
//            pt.AddAttributeOption(def.Key, "flexible", i++);
//            pt.AddAttributeOption(def.Key, "moderate", i++);
//            pt.AddAttributeOption(def.Key, "strict", i++);
//            pt.AddAttributeOption(def.Key, "nonref", i++);
//        }
//    }
//    public static class DemoDataSeeder
//    {
//        public static async Task SeedAsync(PeersContext db, CancellationToken ct = default)
//        {
//            // Ensure catalog was seeded
//            var ptPhones = await RequireTypeByPathAsync(db, "/electronics/mobile-phones", ct);
//            var ptFootwear = await RequireTypeByPathAsync(db, "/fashion/footwear", ct);
//            var ptService = await RequireTypeByPathAsync(db, "/services/service-basic", ct);

//            // Users (roots)
//            var ali = await EnsureSellerAsync(db, "yoyo", "+966511111111", ct);
//            var sara = await EnsureSellerAsync(db, "vovo", "+966522222222", ct);

//            // Listings (roots) — idempotent by (SellerId, Slug)
//            var phone = EnsurePhoneListing(db, seller: ali, type: ptPhones, ct);
//            var shoes = EnsureShoesListing(db, seller: ali, type: ptFootwear, ct);
//            var trip = EnsureServiceListing(db, seller: ali, type: ptService, ct);

//            await db.SaveChangesAsync(ct);
//        }

//        // ──────────────────────────────────────────────────────────────────────────
//        // Users (root)
//        private static async Task<Seller> EnsureSellerAsync(PeersContext db, string username, string phoneNumber, CancellationToken ct)
//        {
//            var u = await db.Sellers.FirstOrDefaultAsync(x => x.Username == username, ct);
//            if (u is not null)
//            {
//                return u;
//            }

//            var nafathIdentity = new NafathIdentity("1111111111", null, null, null, null, null);
//            u = Seller.Create(AppUser.CreateTwoFactorAccount(DateTime.UtcNow, username, phoneNumber, "en"), nafathIdentity);
//            db.Sellers.Add(u);
//            return u;
//        }

//        // Helper: find a leaf type by "electronics/mobile-phones" path
//        private static async Task<ProductType> RequireTypeByPathAsync(PeersContext db, string path, CancellationToken ct)
//        {
//            var pt = await db.ProductTypes
//                .AsNoTrackingWithIdentityResolution()
//                .Include(p => p.Index)
//                .Include(p => p.Attributes).ThenInclude(p => ((EnumAttributeDefinition)p).Options)
//                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).AllowedOptions)
//                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(lt => lt.Options)
//                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(t => t.ParentLinks).ThenInclude(l => l.ChildOption)
//                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(t => t.ChildLinks).ThenInclude(l => l.ParentOption)
//                .FirstOrDefaultAsync(p => p.SlugPath == path, ct);

//            if (!pt!.IsSelectable)
//            {
//                throw new InvalidOperationException("Type is not selectable.");
//            }

//            return pt!;
//        }

//        // ──────────────────────────────────────────────────────────────────────────
//        // Listings (root) — Electronics / phone
//        private static Listing EnsurePhoneListing(PeersContext db, Seller seller, ProductType type, CancellationToken ct)
//        {
//            const string Hashtag = "samsung-galaxy-s24-256gb-black";

//            var listing = Listing.Create(
//                seller: seller,
//                productType: type,
//                fulfillment: new FulfillmentPreferences(FulfillmentMethod.PlatformManaged, null, null, null, null, null),
//                title: "Samsung Galaxy S24 — 256GB — Black",
//                description: "Brand-new, sealed. Local warranty.",
//                hashtag: Hashtag,
//                price: 2499m,
//                date: DateTime.UtcNow,
//            shippingProfile: null);
//            db.Listings.Add(listing); // Generate HiLo Id before SetAttributes

//            // Attributes (must match seeded definitions/options)
//            listing.SetAttributes(listing.Snapshot.SnapshotId, new Dictionary<string, AttributeInputDto>
//            {
//                ["made_in"] = new OptionCodeOrScalarString("g-south_korea"),
//                ["brand"] = new OptionCodeOrScalarString("samsung"),
//                ["model"] = new OptionCodeOrScalarString("Galaxy S24"),
//                ["storage_gb"] = new Numeric(256),
//                ["screen_size_in"] = new Numeric(6.2m),
//                ["samsung_knox"] = new OptionCodeOrScalarString("secure"),
//                // Variants:
//                ["color"] = new OptionCodeAxis(["white", "black"]),
//            }, 3, 100, DateTime.UtcNow);

//            return listing;
//        }

//        // Listings (root) — Footwear / shoes
//        private static Listing EnsureShoesListing(PeersContext db, Seller seller, ProductType type, CancellationToken ct)
//        {
//            const string Hashtag = "adidas-runner-eu43-black";

//            var listing = Listing.Create(
//                seller: seller,
//                productType: type,
//                fulfillment: new FulfillmentPreferences(FulfillmentMethod.PlatformManaged, null, null, null, null, null),
//                title: "Adidas Running Shoes — EU 43 — Black",
//                description: "Lightweight mesh upper. Great for daily runs.",
//                hashtag: Hashtag,
//                price: 299m,
//                date: DateTime.UtcNow,
//                shippingProfile: null);
//            db.Listings.Add(listing); // Generate HiLo Id before SetAttributes

//            listing.SetAttributes(listing.Snapshot.SnapshotId, new Dictionary<string, AttributeInputDto>
//            {
//                ["made_in"] = new OptionCodeOrScalarString("g-china"),
//                ["brand"] = new OptionCodeOrScalarString("adidas"),
//                ["subtype"] = new OptionCodeOrScalarString("running"),
//                ["upper_material"] = new OptionCodeOrScalarString("mesh"),
//                ["sole_material"] = new OptionCodeOrScalarString("rubber"),
//                // Variants:
//                ["color"] = new OptionCodeAxis(["white", "black"]),
//                ["size_value_eu"] = new OptionCodeAxis(["43", "41", "42"]),
//            }, 3, 100, DateTime.UtcNow);

//            return listing;
//        }

//        // Listings (root) — Service
//        private static Listing EnsureServiceListing(PeersContext db, Seller seller, ProductType type, CancellationToken ct)
//        {
//            //const string Hashtag = "island-boat-trip-4hrs";

//            var listing = Listing.Create(
//                seller: seller,
//                productType: type,
//                fulfillment: new FulfillmentPreferences(FulfillmentMethod.PlatformManaged, null, null, null, null, null),
//                title: "Island Boat Trip — 4 hours",
//                description: "Includes water & life jackets. Starts from Marina Gate B.",
//                hashtag: null,
//                price: 599m,
//                date: DateTime.UtcNow,
//                shippingProfile: null);
//            db.Listings.Add(listing); // Generate HiLo Id before SetAttributes

//            listing.SetAttributes(listing.Snapshot.SnapshotId, new Dictionary<string, AttributeInputDto>
//            {
//                ["service_category"] = new OptionCodeOrScalarString("trip"),
//                ["location_kind"] = new OptionCodeOrScalarString("provider"),
//                ["service_area"] = new OptionCodeOrScalarString("Jeddah Corniche"),
//                ["duration_min"] = new Numeric(240),
//                ["capacity"] = new Numeric(6),
//                ["lead_time_hours"] = new Numeric(24),
//                ["cancellation_policy"] = new OptionCodeOrScalarString("moderate"),
//            }, 3, 100, DateTime.UtcNow);

//            return listing;
//        }
//    }
//    public static class PostQueriesForBuyer
//    {
//        // -------------------- DTOs --------------------
//        public sealed record AuthorDto(int Id, string Handle, string DisplayName);
//        public sealed record MediaDto(string Url, string Kind, int SortOrder);

//        public sealed record AttributeDto(
//            string Key,
//            string Name,
//            AttributeKind Type,
//            string? EnumOptionCode,
//            string? EnumOptionLabel,
//            string? LookupOptionCode,
//            string? LookupOptionLabel,
//            string? Value,
//            string? Unit
//        );

//        public sealed record AxisDto(string Key, string Label, string? Unit, AxisOptionDto[] Options);
//        public sealed record AxisOptionDto(string Code, string Label);
//        public sealed record VariantLeafDto(int Id, string SkuCode, decimal Price, int? Stock);

//        public sealed record TypeNodeDto(int Id, string Name, string Slug);
//        public sealed record TypeInfoDto(TypeNodeDto Leaf, TypeNodeDto[] Breadcrumb);

//        public sealed record ProductSummaryDto(
//            int ListingId,
//            string Title,
//            string? Description,
//            decimal Price,
//            TypeInfoDto Type,
//            bool IsPublished,
//            AttributeDto[] Attributes,
//            AxisDto[] Axes,
//            Dictionary<string, VariantLeafDto> VariantIndex
//        );

//        // -------------------- Query --------------------
//        public static async Task<ProductSummaryDto?> GetListingDetailsAsync(
//            PeersContext db, int listingId, CancellationToken ct = default)
//        {
//            ProductSummaryDto? product = null;

//            var listingRaw = await db.Listings
//                .AsNoTracking()
//                .Where(p => p.Id == listingId)
//                .Select(p => new
//                {
//                    p.Id,
//                    p.ProductTypeId,
//                    p.Title,
//                    p.Description,
//                    p.State,
//                    p.BasePrice,
//                    NonVariant = p.Attributes
//                        .OrderBy(a => a.Position)
//                        .Select(a => new AttributeDto
//                        (
//                            a.AttributeDefinition.Key,
//                            null,
//                            a.AttributeDefinition.Kind,
//                            a.EnumAttributeOption!.Code,
//                            null,
//                            a.LookupOption!.Code,
//                            null,
//                            a.Value,
//                            ((IntAttributeDefinition)a.AttributeDefinition)!.Config.Unit ??
//                            ((DecimalAttributeDefinition)a.AttributeDefinition)!.Config.Unit
//                        ))
//                        .ToArray(),
//                    Variants = p.Variants
//                        .Select(p => new
//                        {
//                            p.Id,
//                            p.VariantKey,
//                            p.SkuCode,
//                            p.Price,
//                            p.StockQty,
//                            Attributes = p.Attributes
//                                .OrderBy(p => p.Position)
//                                .Select(va => new
//                                {
//                                    DefKey = va.AttributeDefinition.Key,
//                                    DefLabel = (string?)null,
//                                    DefUnit = ((IntAttributeDefinition)va.AttributeDefinition)!.Config.Unit ??
//                                              ((DecimalAttributeDefinition)va.AttributeDefinition)!.Config.Unit,
//                                    OptCode = va.EnumAttributeOption.Code,
//                                    OptLabel = (string?)null,
//                                    OptPos = va.EnumAttributeOption.Position,
//                                })
//                                .ToArray(),
//                        }),
//                })
//                .FirstAsync(ct);

//            var l = new
//            {
//                listingRaw.Id,
//                listingRaw.ProductTypeId,
//                listingRaw.Title,
//                listingRaw.Description,
//                listingRaw.State,
//                listingRaw.BasePrice,
//                listingRaw.NonVariant,
//                Axes = listingRaw.Variants
//                    .SelectMany(p => p.Attributes)
//                    .Distinct()
//                    .GroupBy(x => new { x.DefKey, x.DefUnit, x.DefLabel })
//                    .Select(g => new AxisDto
//                    (
//                        g.Key.DefKey,
//                        g.Key.DefLabel,
//                        g.Key.DefUnit,
//                        [.. g
//                            .OrderBy(o => o.OptPos)
//                            .Select(o => new AxisOptionDto
//                            (
//                                o.OptCode,
//                                o.OptLabel
//                            ))]
//                    ))
//                    .ToArray(),
//                VariantIndex = listingRaw.Variants
//                    .ToDictionary(
//                        x => x.VariantKey,
//                        x => new VariantLeafDto
//                        (
//                            x.Id,
//                            x.SkuCode,
//                            x.Price,
//                            x.StockQty
//                        ))
//            };

//            var chain = await db.ProductTypeAncestors(l.ProductTypeId)
//                .AsNoTrackingWithIdentityResolution()
//                .ToArrayAsync(ct);

//            var listingProductType = chain[^1];

//            var breadcrumb = chain.Select(r => new TypeNodeDto(r.Id, null, r.SlugPath)).ToArray();
//            var typeInfo = new TypeInfoDto(breadcrumb[^1], breadcrumb);

//            product = new ProductSummaryDto(
//                ListingId: l.Id,
//                Title: l.Title,
//                Description: l.Description,
//                Price: l.BasePrice,
//                Type: typeInfo,
//                IsPublished: l.State == ListingState.Published,
//                Attributes: l.NonVariant,
//                Axes: l.Axes,
//                VariantIndex: l.VariantIndex
//            );

//            // 3) Final DTO
//            return product;
//        }
//    }
//    public static class PostQueriesForSeller
//    {
//        public sealed record EditorAttributeDto(
//            string Key,
//            string Label,
//            AttributeKind Kind,
//            bool IsVariant,
//            bool Required,
//            int Position,
//            string? DependsOnKey,
//            object? Selected,                  // { code,label } OR { value } OR null
//            EditorOptionDto[]? Options,        // for unscoped attributes
//            ScopedGroupDto[]? ScopedGroups,    // for scoped attributes; grouped by parent option
//            LookupOptionDto[]? LookupOptions
//        );

//        public sealed record EditorOptionDto(string Code, string Label, int Position);
//        public sealed record ScopedGroupDto(string ParentCode, EditorOptionDto[] Options);
//        public sealed record LookupOptionDto(string Code, string Label);

//        public static async Task<object?> GetListingEditorAsync(
//            PeersContext db, int listingId, CancellationToken ct = default)
//        {
//            var q = await db.Listings
//                .AsNoTracking()
//                .Where(p => p.Id == listingId)
//                .Select(p => new
//                {
//                    p.Id,
//                    p.Title,
//                    p.Description,
//                    p.BasePrice,
//                    p.State,
//                    p.ProductTypeId,
//                    // 1) Pull all defs for this version
//                    Defs = p.ProductType.Attributes
//                        .OrderBy(d => d.Position)
//                        .Select(d => new
//                        {
//                            d.Id,
//                            d.Key,
//                            Label = (string?)null,
//                            d.Kind,
//                            IsVariant = d.Kind == AttributeKind.Enum && ((EnumAttributeDefinition)d)!.IsVariant,
//                            d.IsRequired,
//                            d.Position,
//                            DependsOnKey = ((EnumAttributeDefinition)d)!.DependsOn.Key,

//                            // 2) All options for this def (both unscoped & scoped)
//                            Opts = ((EnumAttributeDefinition)d)!.Options
//                                .OrderBy(o => o.Position)
//                                .Select(o => new
//                                {
//                                    o.Code,
//                                    Label = (string?)null,
//                                    o.Position,
//                                    ParentCode = o.ParentOption!.Code,
//                                })
//                                .ToArray(),
//                            // Actually must get from PT lineage not just current PT!
//                            LookupOpts = d.Kind == AttributeKind.Lookup ? p.ProductType.Index.Hydrated.LookupByCode
//                                .Where(l => l.Key == ((LookupAttributeDefinition)d)!.Key)
//                                .SelectMany(p => p.Value)
//                                .Select(l => new
//                                {
//                                    l.Value.Code,
//                                    Label = (string?)null,
//                                })
//                                .ToArray() : null,
//                            // 3) Current selection for this listing (non-variant side)
//                            Sel = p.Attributes
//                                .Where(a => a.AttributeDefinitionId == d.Id)
//                                .Select(a => new
//                                {
//                                    OptCode = a.EnumAttributeOption!.Code,
//                                    OptLabel = (string?)null,
//                                    LookupKey = a.LookupOption!.Code,
//                                    LookupLabel = (string?)null,
//                                    Val = a.Value
//                                })
//                                .FirstOrDefault()
//                        })
//                        .ToArray(),

//                    // 4) Variants for the grid / index
//                    Variants = p.Variants.Select(v => new
//                    {
//                        v.Id,
//                        v.VariantKey,
//                        v.SkuCode,
//                        v.Price,
//                        v.StockQty
//                    }).ToArray()
//                })
//                .FirstOrDefaultAsync(ct);

//            if (q is null)
//            {
//                return null;
//            }

//            // 5) Build editor attributes (group scoped options by immediate parent)
//            var editorAttrs = q.Defs.Select(d =>
//            {
//                var unscoped = d.DependsOnKey == null
//                    ? d.Opts
//                        .Where(o => o.ParentCode == null)
//                        .Select(o => new EditorOptionDto(o.Code, o.Label, o.Position))
//                        .ToArray()
//                    : null;

//                var scoped = d.DependsOnKey != null
//                    ? d.Opts
//                        .Where(o => o.ParentCode != null)
//                        .GroupBy(o => o.ParentCode!)
//                        .Select(g => new ScopedGroupDto(
//                            ParentCode: g.Key,
//                            Options: g
//                                .Select(x => new EditorOptionDto(x.Code, x.Label, x.Position))
//                                .ToArray()))
//                        .ToArray()
//                    : null;

//                object? selected = d.Kind == AttributeKind.Enum
//                    ? (d.Sel?.OptCode == null ? null : new { key = d.Sel.OptCode, label = d.Sel.OptLabel })
//                    : (d.Sel?.LookupKey == null
//                        ? (d.Sel?.Val == null ? null : new { value = d.Sel.Val })
//                        : new { key = d.Sel.LookupKey, label = d.Sel.LookupLabel });

//                return new EditorAttributeDto(
//                        Key: d.Key,
//                        Label: d.Label,
//                        Kind: d.Kind,
//                        IsVariant: d.IsVariant,
//                        Required: d.IsRequired,
//                        Position: d.Position,
//                        DependsOnKey: d.DependsOnKey,
//                        Selected: selected,
//                        Options: unscoped,
//                        ScopedGroups: scoped,
//                        LookupOptions: d.Kind is AttributeKind.Lookup ? d.LookupOpts.Select(l => new LookupOptionDto(l.Code, l.Label)).ToArray() : null
//                    );
//            })
//            .ToArray();

//            // 6) Final 1-shot editor payload
//            return new
//            {
//                listingId = q.Id,
//                title = q.Title,
//                description = q.Description,
//                basePrice = q.BasePrice,
//                isPublished = q.State == ListingState.Published,

//                attributes = editorAttrs,         // all variant + non-variant, with scopedGroups if needed
//                variantIndex = q.Variants.ToDictionary(
//                    x => x.VariantKey,
//                    x => new { id = x.Id, skuCode = x.SkuCode, price = x.Price, stock = x.StockQty }
//                )
//            };
//        }
//    }

//    private static LookupType CreateLookupWithOptions(string key, LookupConstraintMode constraintMode, bool allowVariants, string[] codes)
//    {
//        var lookupType = new LookupType(key, constraintMode, allowVariants);
//        foreach (var code in codes)
//        {
//            lookupType.CreateOption(code);
//        }

//        return lookupType;
//    }
//}
