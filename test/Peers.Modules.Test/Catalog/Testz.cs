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

//[Collection(nameof(IntegrationTestBaseCollection))]
//public class Testz : IntegrationTestBase
//{
//    [Fact]
//    public async Task T1()
//    {
//        await SeedLookupsAsync();
//        await SeedProductTypeAsync();
//        await SeedListingAsync();
//    }

//    public Task SeedLookupsAsync() => ExecuteDbContextAsync(async db =>
//    {
//        var brands = new string[] { "g_apple", "g_samsung" };
//        var models = new string[]
//        {
//            // Apple
//            "g_iphone_17", "g_iphone_17_pro", "g_iphone_17_pro_max", "g_macbook_pro_16",
//            // Samsung
//            "g_galaxy_s25", "g_galaxy_s25_plus", "g_galaxy_s25_ultra", "g_dux1e_4k_tv"
//        };
//        var colors = new string[]
//        {
//            // iPhone 17
//            "g_mist_blue", "g_lavender", "g_white", "g_black",
//            // iPhone 17 Pro/Pro Max
//            "g_cosmic_orange", "g_deep_blue", "g_silver",
//            // Galaxy S25/S25+
//            "g_silver_shadow", "g_icyblue", "g_mint", "g_navy",
//            // Galaxy S25 Ultra
//            "g_titanium_silver_blue", "g_titanium_black", "g_titanium_grey", "g_titanium_silver_white",
//            // iPhone 17 Pro Max 1TB exclusive color
//            "g_gold",
//            // Galaxy S25 Ultra 1TB exclusive color
//            "g_phantom_black"
//        };
//        var ramSizes = new string[]
//        {
//            // iPhone 17/Pro/Pro Max | Galaxy S25/S25+/Ultra
//            "g_256gb",
//            // iPhone 17 Pro/Pro Max | Galaxy S25+/Ultra
//            "g_512gb",
//            // iPhone 17 Pro Max | Galaxy S25 Ultra
//            "g_1tb"
//        };

//        var brandLookup = CreateLookupWithOptions("g_brand", LookupConstraintMode.Open, false, brands);
//        var modelLookup = CreateLookupWithOptions("g_model", LookupConstraintMode.Open, false, models);
//        var mobileColorLookup = CreateLookupWithOptions("g_color", LookupConstraintMode.Open, true, colors);
//        var mobileRamSizeLookup = CreateLookupWithOptions("g_ram_size", LookupConstraintMode.Open, true, ramSizes);

//        brandLookup.LinkOptions("g_apple", modelLookup, ["g_iphone_17", "g_iphone_17_pro", "g_iphone_17_pro_max", "g_macbook_pro_16"]);
//        brandLookup.LinkOptions("g_samsung", modelLookup, ["g_galaxy_s25", "g_galaxy_s25_plus", "g_galaxy_s25_ultra", "g_dux1e_4k_tv"]);

//        modelLookup.LinkOptions("g_iphone_17", mobileColorLookup, ["g_mist_blue", "g_lavender", "g_white", "g_black"]);
//        modelLookup.LinkOptions("g_iphone_17_pro", mobileColorLookup, ["g_cosmic_orange", "g_deep_blue", "g_silver"]);
//        modelLookup.LinkOptions("g_iphone_17_pro_max", mobileColorLookup, ["g_cosmic_orange", "g_deep_blue", "g_silver", "g_gold"]);

//        modelLookup.LinkOptions("g_galaxy_s25", mobileColorLookup, ["g_silver_shadow", "g_icyblue", "g_mint", "g_navy"]);
//        modelLookup.LinkOptions("g_galaxy_s25_plus", mobileColorLookup, ["g_silver_shadow", "g_icyblue", "g_mint", "g_navy"]);
//        modelLookup.LinkOptions("g_galaxy_s25_ultra", mobileColorLookup, ["g_titanium_silver_blue", "g_titanium_black", "g_titanium_grey", "g_titanium_silver_white", "g_phantom_black"]);

//        modelLookup.LinkOptions("g_iphone_17", mobileRamSizeLookup, ["g_256gb"]);
//        modelLookup.LinkOptions("g_iphone_17_pro", mobileRamSizeLookup, ["g_256gb", "g_512gb"]);
//        modelLookup.LinkOptions("g_iphone_17_pro_max", mobileRamSizeLookup, ["g_256gb", "g_512gb", "g_1tb"]);

//        modelLookup.LinkOptions("g_galaxy_s25", mobileRamSizeLookup, ["g_256gb"]);
//        modelLookup.LinkOptions("g_galaxy_s25_plus", mobileRamSizeLookup, ["g_256gb", "g_512gb"]);
//        modelLookup.LinkOptions("g_galaxy_s25_ultra", mobileRamSizeLookup, ["g_256gb", "g_512gb", "g_1tb"]);

//        mobileRamSizeLookup.LinkOptions("g_256gb", mobileColorLookup, colors[..^2]);
//        mobileRamSizeLookup.LinkOptions("g_512gb", mobileColorLookup, colors[4..^2]);
//        mobileRamSizeLookup.LinkOptions("g_1tb", mobileColorLookup, colors[^2..]);

//        db.LookupTypes.AddRange(brandLookup, modelLookup, mobileColorLookup, mobileRamSizeLookup);
//        await db.SaveChangesAsync();
//    });

//    public Task SeedProductTypeAsync() => ExecuteDbContextAsync(async db =>
//    {
//        var brandLookup = db.LookupTypes
//            .Include(p => p.Options)
//            .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//            .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//            .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//            .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//            .FirstOrDefault(p => p.Key == "g_brand");
//        var modelLookup = db.LookupTypes
//            .Include(p => p.Options)
//            .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//            .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//            .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//            .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//            .FirstOrDefault(p => p.Key == "g_model");
//        var colorLookup = db.LookupTypes.Include(p => p.Options)
//            .Include(p => p.Options)
//            .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//            .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//            .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//            .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//            .FirstOrDefault(p => p.Key == "g_color");
//        var ramSizeLookup = db.LookupTypes.Include(p => p.Options)
//            .Include(p => p.Options)
//            .Include(p => p.ParentLinks).ThenInclude(p => p.ChildType)
//            .Include(p => p.ParentLinks).ThenInclude(p => p.ChildOption)
//            .Include(p => p.ChildLinks).ThenInclude(p => p.ParentType)
//            .Include(p => p.ChildLinks).ThenInclude(p => p.ParentOption)
//            .FirstOrDefault(p => p.Key == "g_ram_size");

//        var c = 1;
//        var root = ProductType.CreateRoot(ProductTypeKind.Physical, "Electronics");
//        root.DefineLookupAttribute("brand", isRequired: true, isVariant: false, position: 1, brandLookup);
//        root.DefineEnumAttribute("condition", isRequired: true, isVariant: false, position: 2);
//        foreach (var optCode in new[] { "new", "used", "refurbished" })
//        {
//            root.AddAttributeOption("condition", optCode, c++);
//        }

//        root.Publish();

//        var mobilePhones = root.AddChild("Mobile Phones", isSelectable: true, copyAttributes: true);
//        mobilePhones.DefineLookupAttribute("model", isRequired: true, isVariant: false, position: 3, modelLookup);
//        mobilePhones.DefineLookupAttribute("color", isRequired: true, isVariant: true, position: 4, colorLookup);
//        mobilePhones.DefineLookupAttribute("ram_size", isRequired: true, isVariant: true, position: 5, ramSizeLookup);

//        foreach (var mobilePhoneModel in modelLookup.Options.Where(p => p.Code != "g_macbook_pro_16" && p.Code != "g_dux1e_4k_tv"))
//        {
//            mobilePhones.AddAllowedLookup("model", mobilePhoneModel);
//        }

//        //////////////////////////
//        // Mobile phone attributes
//        //////////////////////////
//        mobilePhones.DefineAttribute("screen_size", AttributeKind.Decimal, isRequired: false, isVariant: false, position: 6, unit: "inch", min: 3.0m, max: 10.0m, step: 0.1m);
//        mobilePhones.DefineAttribute("battery_capacity", AttributeKind.Int, isRequired: false, isVariant: false, position: 7, unit: "mAh", min: 500m, max: 10000m, step: 100m);
//        mobilePhones.DefineAttribute("has_5g", AttributeKind.Bool, isRequired: false, isVariant: false, position: 8);
//        mobilePhones.DefineAttribute("release_date", AttributeKind.Date, isRequired: false, isVariant: false, position: 9);
//        //////////////////////////

//        mobilePhones.Publish();

//        db.ProductTypes.Add(root);
//        await db.SaveChangesAsync();
//    });

//    public Task SeedListingAsync() => ExecuteDbContextAsync(async db =>
//    {
//        var seller = await EnsureSellerAsync(db, "Dapp", "+966511111111");
//        var pt = await db.ProductTypes
//            .AsSplitQuery()
//            .Include(p => p.Index)
//            .Include(p => p.Attributes).ThenInclude(p => ((EnumAttributeDefinition)p).Options)
//            .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).AllowedOptions)
//            .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.Options)
//            .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.ParentLinks)
//            .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.ChildLinks)
//            .FirstOrDefaultAsync(p => p.SlugPath == "/electronics/mobile-phones");

//        var listing = Listing.Create(
//            seller: seller,
//            productType: pt,
//                fulfillment: new FulfillmentPreferences(FulfillmentMethod.PlatformManaged, null, null, null, null, null),
//            title: "Samsung Galaxy S25 Ultra — 256GB/512GB/1TB — Multi Color",
//            description: "Brand-new, sealed. Local warranty.",
//            hashtag: "samsung-galaxy-s25-ultra-1tb-phantom-black",
//            price: 2499m,
//            date: DateTime.UtcNow,
//            shippingProfile: null);
//        db.Listings.Add(listing); // Generate HiLo Id before SetAttributes

//        listing.SetAttributes(
//            snapshotId: listing.Snapshot.SnapshotId,
//            inputs: new Dictionary<string, AttributeInputDto>
//            {
//                ["brand"] = new OptionCodeOrScalarString("g_samsung"),
//                ["model"] = new OptionCodeOrScalarString("g_galaxy_s25_ultra"),
//                ["color"] = new OptionCodeAxis(["g_titanium_silver_blue", "g_titanium_black", "g_titanium_grey", "g_titanium_silver_white", "g_phantom_black"/*, "g_gold"*/]),
//                ["ram_size"] = new OptionCodeAxis(["g_256gb", "g_512gb", "g_1tb"]),
//                ["screen_size"] = new Numeric(6.8m),
//                ["battery_capacity"] = new Numeric(5000),
//                ["has_5g"] = new Bool(true),
//                ["release_date"] = new Date(new DateOnly(2024, 2, 1)),
//                ["condition"] = new OptionCodeOrScalarString("new"),
//            },
//            variantAxesCap: 3,
//            skuCap: 200,
//            date: DateTime.UtcNow);

//        await db.SaveChangesAsync();

//        //pt.Index.Hydrated.TryGetLookupAllowedChildren("brand", "model", "g_apple", out var xx);
//        //pt.Index.Hydrated.IsChildCodeReachableFromParents("color", "g_gold", new Dictionary<string, AttributeInputDto>
//        //{
//        //    ["brand"] = new OptionCodeOrScalarString("g_samsung"),
//        //    ["model"] = new OptionCodeOrScalarString("g_galaxy_s25_ultra"),
//        //    ["color"] = new OptionCodeAxis(["g_titanium_silver_blue", "g_titanium_black", "g_titanium_grey", "g_titanium_silver_white", "g_phantom_black", "g_gold"]),
//        //    ["ram_size"] = new OptionCodeAxis(["g_256gb", "g_512gb", "g_1tb"]),
//        //    ["screen_size"] = new Numeric(6.8m),
//        //    ["battery_capacity"] = new Numeric(5000),
//        //    ["has_5g"] = new Bool(true),
//        //    ["release_date"] = new Date(new DateOnly(2024, 2, 1)),
//        //    ["condition"] = new OptionCodeOrScalarString("new"),
//        //});
//        listing.Publish();
//        await db.SaveChangesAsync();
//    });

//    private static async Task<Seller> EnsureSellerAsync(PeersContext db, string username, string phoneNumber)
//    {
//        var u = await db.Sellers.FirstOrDefaultAsync(x => x.Username == username);
//        if (u is not null)
//        {
//            return u;
//        }

//        var nafathIdentity = new NafathIdentity("1111111111", null, null, null, null, null);
//        u = Seller.Create(AppUser.CreateTwoFactorAccount(DateTime.UtcNow, username, phoneNumber, "en"), nafathIdentity);
//        db.Sellers.Add(u);
//        return u;
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
