//using Peers.Core.Localization.Infrastructure;
//using Peers.Modules.Catalog.Domain;
//using Peers.Modules.Catalog.Domain.Attributes;
//using Peers.Modules.Catalog.Domain.Translations;
//using Peers.Modules.Catalog.Utils;
//using Peers.Modules.Lookup.Domain;

//namespace Peers.Modules.Test.Catalog.Utils;

//public sealed class AttributeSchemaClonerTests
//{
//    [Fact]
//    public void CopyFrom_Clones_AllKinds_DependentAndNonDependentEnums_WithTranslations()
//    {
//        // Arrange
//        var source = ProductType.CreateRoot(ProductTypeKind.Physical, "PT-Source");
//        var target = ProductType.CreateRoot(ProductTypeKind.Physical, "PT-Target");

//        var brandLookupType = CreateLookupWithOptions("brand", LookupConstraintMode.RequireAllowList, false, ["apple", "samsung"]);
//        var modelLookupType = CreateLookupWithOptions("model", default, default, ["iphone_14", "galaxy_s22"]);
//        //var lookupLinks = new List<LookupLink>
//        //{
//        //    new() { ParentValue = brandLookupType.Values[0], ChildValue = modelLookupType.Values[0] },
//        //    new() { ParentValue = brandLookupType.Values[1], ChildValue = modelLookupType.Values[1] }
//        //};

//        // Lookup (entity reference) with translations
//        source
//            .DefineLookupAttribute(
//                key: "brand",
//                isRequired: true,
//                isVariant: false,
//                position: 0,
//                lookupType: brandLookupType
//            )
//            .UpsertTranslations(("en", "Brand", null), ("ar", "الماركة", null));

//        source
//            .DefineLookupAttribute(
//                key: "device_model",
//                isRequired: true,
//                isVariant: false,
//                position: 1,
//                lookupType: modelLookupType
//            )
//            .UpsertTranslations(("en", "Device Model", null), ("ar", "موديل الجهاز", null));

//        // String (regex) with translations
//        source
//            .DefineStringAttribute(
//                key: "model",
//                isRequired: true,
//                position: 2,
//                regex: "^[A-Z].+$"
//            )
//            .UpsertTranslations(("en", "Model", null), ("ar", "الموديل", null));

//        // Int (unit + min/max) with translations
//        source
//            .DefineIntAttribute(
//                key: "weight",
//                isRequired: true,
//                isVariant: false,
//                position: 3,
//                unit: "g",
//                min: 0, max: 10_000
//            )
//            .UpsertTranslations(("en", "Weight", "g"));

//        // Decimal (unit + min/max) with translations
//        source
//            .DefineDecimalAttribute(
//                key: "price",
//                isRequired: true,
//                isVariant: false,
//                position: 4,
//                unit: "sar",
//                min: 0.5m, max: 99_999.99m
//            )
//            .UpsertTranslations(("en", "Price", "SAR"));

//        // String WITHOUT translations (to hit "no translations" branch)
//        source.DefineStringAttribute(
//            key: "material",
//            isRequired: false,
//            position: 5
//        );

//        // Enum (no dependency), options both with and without translations
//        source
//            .DefineEnumAttribute(
//                key: "color",
//                isRequired: false,
//                position: 6,
//                isVariant: true
//            )
//            .UpsertTranslations(("en", "Color", null));

//        source
//            .AddAttributeOption("color", "red", position: 1)
//            .UpsertTranslations(("en", "Red"), ("ar", "أحمر"));

//        source
//            .AddAttributeOption("color", "blue", position: 2)
//            .UpsertTranslations(("en", "Blue"));

//        source.AddAttributeOption("color", "green", position: 3);
//        // no translations for "green" (to hit "no translations" branch)

//        // Dependent Enum: shade depends on color; each option is scoped to a parent option
//        source
//            .DefineDependentAttribute(
//                parentKey: "color",
//                key: "shade",
//                isRequired: false,
//                position: 7,
//                isVariant: false
//            )
//            .UpsertTranslations(("en", "Shade", null));

//        source
//            .AddAttributeOption("shade", "light", position: 1, parentOptionCode: "red")
//            .UpsertTranslations(("en", "Light"));

//        source
//            .AddAttributeOption("shade", "dark", position: 2, parentOptionCode: "blue")
//            .UpsertTranslations(("en", "Dark"));

//        // Act
//        AttributeSchemaCloner.CopyFrom(source, target);

//        // Assert: counts
//        Assert.Equal(target.Attributes.Count, source.Attributes.Count);

//        // Assert: Lookup "brand"
//        var tBrand = GetDef(target, "brand") as LookupAttributeDefinition;
//        Assert.NotNull(tBrand);
//        Assert.Equal(AttributeKind.Lookup, tBrand.Kind);
//        Assert.True(tBrand.IsRequired);
//        Assert.Equal(0, tBrand.Position);
//        Assert.Same(brandLookupType, tBrand.LookupType);
//        Assert.Equal(2, tBrand.Translations.Count);
//        Assert.Single(tBrand.Translations, x => x.LangCode == "en" && x.Name == "Brand");
//        Assert.Single(tBrand.Translations, x => x.LangCode == "ar" && x.Name == "الماركة");

//        // Assert: Dependent Lookup "device-model"
//        var tDeviceModel = GetDef(target, "device_model") as LookupAttributeDefinition;
//        Assert.NotNull(tDeviceModel);
//        Assert.Equal(AttributeKind.Lookup, tDeviceModel.Kind);
//        Assert.True(tDeviceModel.IsRequired);
//        Assert.Equal(1, tDeviceModel.Position);
//        Assert.Same(modelLookupType, tDeviceModel.LookupType);
//        Assert.Equal(2, tDeviceModel.Translations.Count);
//        Assert.Single(tDeviceModel.Translations, x => x.LangCode == "en" && x.Name == "Device Model");
//        Assert.Single(tDeviceModel.Translations, x => x.LangCode == "ar" && x.Name == "موديل الجهاز");

//        // Assert: String "model"
//        var tModel = GetDef(target, "model") as StringAttributeDefinition;
//        Assert.NotNull(tModel);
//        Assert.Equal(AttributeKind.String, tModel.Kind);
//        Assert.True(tModel.IsRequired);
//        Assert.Equal(2, tModel.Position);
//        Assert.Equal("^[A-Z].+$", tModel.Config.Regex);
//        Assert.Equal(2, tModel.Translations.Count);
//        Assert.Single(tModel.Translations, x => x.LangCode == "en" && x.Name == "Model");
//        Assert.Single(tModel.Translations, x => x.LangCode == "ar" && x.Name == "الموديل");

//        // Assert: Int "weight"
//        var tWeight = GetDef(target, "weight") as IntAttributeDefinition;
//        Assert.NotNull(tWeight);
//        Assert.Equal(AttributeKind.Int, tWeight.Kind);
//        Assert.True(tWeight.IsRequired);
//        Assert.Equal(3, tWeight.Position);
//        Assert.Equal("g", tWeight.Config.Unit);
//        Assert.Equal(0, tWeight.Config.Min);
//        Assert.Equal(10_000, tWeight.Config.Max);
//        var wtr = Assert.Single(tWeight.Translations);
//        Assert.Equal("en", wtr.LangCode);
//        Assert.Equal("Weight", wtr.Name);
//        Assert.Equal("g", wtr.Unit);

//        // Assert: Decimal "price"
//        var tPrice = GetDef(target, "price") as DecimalAttributeDefinition;
//        Assert.NotNull(tPrice);
//        Assert.Equal(AttributeKind.Decimal, tPrice.Kind);
//        Assert.True(tPrice.IsRequired);
//        Assert.Equal(4, tPrice.Position);
//        Assert.Equal("sar", tPrice.Config.Unit);
//        Assert.Equal(0.5m, tPrice.Config.Min);
//        Assert.Equal(99_999.99m, tPrice.Config.Max);
//        var ptr = Assert.Single(tPrice.Translations);
//        Assert.Equal("en", ptr.LangCode);
//        Assert.Equal("Price", ptr.Name);
//        Assert.Equal("SAR", ptr.Unit);

//        // Assert: String "material" (no translations branch hit)
//        var tMaterial = GetDef(target, "material") as StringAttributeDefinition;
//        Assert.NotNull(tMaterial);
//        Assert.Equal(AttributeKind.String, tMaterial.Kind);
//        Assert.Empty(tMaterial.Translations);

//        // Assert: Enum "color" (no dependency)
//        var tColor = GetDef(target, "color") as EnumAttributeDefinition;
//        Assert.NotNull(tColor);
//        Assert.Equal(AttributeKind.Enum, tColor.Kind);
//        Assert.True(tColor.IsVariant);
//        Assert.Equal(6, tColor.Position);
//        Assert.Single(tColor.Translations, x => x.LangCode == "en" && x.Name == "Color");

//        // Options: red/blue/green; red & blue have translations; green none
//        var red = tColor.Options.Single(o => o.Code == "red");
//        var blue = tColor.Options.Single(o => o.Code == "blue");
//        var green = tColor.Options.Single(o => o.Code == "green");

//        Assert.Null(red.ParentOption);
//        Assert.Null(blue.ParentOption);
//        Assert.Null(green.ParentOption);

//        Assert.Equal(2, red.Translations.Count);
//        Assert.Single(red.Translations, x => x.LangCode == "en" && x.Name == "Red");
//        Assert.Single(red.Translations, x => x.LangCode == "ar" && x.Name == "أحمر");

//        var btr = Assert.Single(blue.Translations);
//        Assert.Equal("en", btr.LangCode);
//        Assert.Equal("Blue", btr.Name);

//        Assert.Empty(green.Translations);

//        // Assert: Dependent enum "shade" (depends on color) and scoped options
//        var tShade = GetDef(target, "shade") as EnumAttributeDefinition;
//        Assert.NotNull(tShade);
//        Assert.Equal(AttributeKind.Enum, tShade.Kind);
//        Assert.False(tShade.IsVariant);
//        Assert.Equal(7, tShade.Position);
//        Assert.NotNull(tShade.DependsOn);
//        Assert.Equal("color", tShade.DependsOn.Key);
//        Assert.Single(tShade.Translations, x => x.LangCode == "en" && x.Name == "Shade");

//        var light = tShade.Options.Single(o => o.Code == "light");
//        var dark = tShade.Options.Single(o => o.Code == "dark");

//        Assert.NotNull(light.ParentOption);
//        Assert.NotNull(dark.ParentOption);

//        Assert.Equal("color", light.ParentOption.EnumAttributeDefinition.Key);
//        Assert.Equal("color", dark.ParentOption.EnumAttributeDefinition.Key);

//        Assert.Equal("red", light.ParentOption.Code);
//        Assert.Equal("blue", dark.ParentOption.Code);

//        Assert.Single(light.Translations, x => x.LangCode == "en" && x.Name == "Light");
//        Assert.Single(dark.Translations, x => x.LangCode == "en" && x.Name == "Dark");
//    }

//    [Fact]
//    public void CopyFrom_WithEmptySource_NoAttributes_CreatesNothing()
//    {
//        var source = ProductType.CreateRoot(ProductTypeKind.Physical, "Empty");
//        var target = ProductType.CreateRoot(ProductTypeKind.Physical, "Target");

//        AttributeSchemaCloner.CopyFrom(source, target);

//        Assert.Empty(target.Attributes!);
//    }

//    // ---------- Helpers ----------

//    private static AttributeDefinition GetDef(ProductType pt, string key) =>
//        pt.Attributes.Single(a => a.Key == key);

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
