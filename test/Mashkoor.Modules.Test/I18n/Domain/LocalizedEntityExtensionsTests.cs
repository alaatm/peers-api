using System.Globalization;
using Mashkoor.Core.Domain;
using Mashkoor.Modules.I18n.Domain;

namespace Mashkoor.Modules.Test.I18n.Domain;

public class LocalizedEntityExtensionsTests
{
    [Theory]
    [InlineData("en-US", "en_name")]
    [InlineData("ar-SA", "ar_name")]
    public void LocalizedName_returns_localized_name(string culture, string expected)
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"), (ar, "ar_name"));
        entity.AddOrUpdateTranslations(name);

        // Act
        var currentCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        var localizedName = entity.LocalizedName();
        Thread.CurrentThread.CurrentCulture = currentCulture;

        // Assert
        Assert.Equal(expected, localizedName);
    }

    [Fact]
    public void LocalizedName_returns_NA_when_no_translation_exist()
    {
        // Arrange
        var entity = new TestEntity() { Translations = [] };

        // Act
        var localizedName = entity.LocalizedName();

        // Assert
        Assert.Equal("N/A", localizedName);
    }

    [Theory]
    [InlineData("en", "en_name")]
    [InlineData("ar", "ar_name")]
    public void NameForLang_returns_name_for_requested_lang(string lang, string expected)
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"), (ar, "ar_name"));
        entity.AddOrUpdateTranslations(name);

        // Act
        var localizedName = entity.NameForLang(lang);

        // Assert
        Assert.Equal(expected, localizedName);
    }

    [Fact]
    public void NameForLang_returns_NA_when_no_translation_exist()
    {
        // Arrange
        var entity = new TestEntity() { Translations = [] };

        // Act
        var localizedName = entity.NameForLang("en");

        // Assert
        Assert.Equal("N/A", localizedName);
    }

    [Fact]
    public void AddOrUpdateTranslations_adds_missing_translations()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"));
        entity.AddOrUpdateTranslations(name);
        Assert.Single(entity.Translations);
        name = TranslatedField.CreateList((ar, "ar_name"));

        // Act
        entity.AddOrUpdateTranslations(name);

        // Assert
        Assert.Equal(2, entity.Translations.Count);
        AssertTranslation(entity.Translations.ElementAt(0), en, "en_name");
        AssertTranslation(entity.Translations.ElementAt(1), ar, "ar_name");
    }

    [Fact]
    public void AddOrUpdateTranslations_updates_existing_translations()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"), (ar, "ar_name"));
        entity.AddOrUpdateTranslations(name);
        Assert.Equal(2, entity.Translations.Count);
        name = TranslatedField.CreateList((ar, "Arabic-Name"));

        // Act
        entity.AddOrUpdateTranslations(name);

        // Assert
        Assert.Equal(2, entity.Translations.Count);
        AssertTranslation(entity.Translations.ElementAt(0), en, "en_name");
        AssertTranslation(entity.Translations.ElementAt(1), ar, "Arabic-Name");
    }

    private static void AssertTranslation(TestEntityTranslation t, Language l, string name)
    {
        Assert.Equal(0, t.EntityId);
        Assert.Same(l.Id, t.LanguageId);
        Assert.Equal(t.Name, name);
    }

    [Fact]
    public void AddOrUpdateTranslations2f_adds_missing_translations()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity2f() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"));
        var descr = TranslatedField.CreateList((en, "en_descr"));
        entity.AddOrUpdateTranslations(name, descr);
        Assert.Single(entity.Translations);
        name = TranslatedField.CreateList((ar, "ar_name"));
        descr = TranslatedField.CreateList((ar, "ar_descr"));

        // Act
        entity.AddOrUpdateTranslations(name, descr);

        // Assert
        Assert.Equal(2, entity.Translations.Count);
        AssertTranslation2f(entity.Translations.ElementAt(0), en, "en_name", "en_descr");
        AssertTranslation2f(entity.Translations.ElementAt(1), ar, "ar_name", "ar_descr");
    }

    [Fact]
    public void AddOrUpdateTranslations2f_updates_existing_translations()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity2f() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"), (ar, "ar_name"));
        var descr = TranslatedField.CreateList((en, "en_descr"), (ar, "ar_descr"));
        entity.AddOrUpdateTranslations(name, descr);
        Assert.Equal(2, entity.Translations.Count);
        name = TranslatedField.CreateList((ar, "Arabic-Name"));
        descr = TranslatedField.CreateList((ar, "Arabic-Descr"));

        // Act
        entity.AddOrUpdateTranslations(name, descr);

        // Assert
        Assert.Equal(2, entity.Translations.Count);
        AssertTranslation2f(entity.Translations.ElementAt(0), en, "en_name", "en_descr");
        AssertTranslation2f(entity.Translations.ElementAt(1), ar, "Arabic-Name", "Arabic-Descr");
    }

    [Fact]
    public void LocalizedNameFields_returns_localized_fields()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"), (ar, "ar_name"));
        entity.AddOrUpdateTranslations(name);

        // Act
        var localizedFields = entity.LocalizedNameFields();

        // Assert
        Assert.Equal(2, localizedFields.Length);
        Assert.Equal(en.Id, localizedFields[0].Language);
        Assert.Equal("en_name", localizedFields[0].Value);
        Assert.Equal(ar.Id, localizedFields[1].Language);
        Assert.Equal("ar_name", localizedFields[1].Value);
    }

    [Fact]
    public void LocalizedNameFields2f_returns_localized_fields()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity2f() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"), (ar, "ar_name"));
        var descr = TranslatedField.CreateList((en, "en_descr"), (ar, "ar_descr"));
        entity.AddOrUpdateTranslations(name, descr);

        // Act
        var localizedFields = entity.LocalizedNameFields2f();

        // Assert
        Assert.Equal(2, localizedFields.Length);
        Assert.Equal(en.Id, localizedFields[0].Language);
        Assert.Equal("en_name", localizedFields[0].Value);
        Assert.Equal(ar.Id, localizedFields[1].Language);
        Assert.Equal("ar_name", localizedFields[1].Value);
    }

    [Fact]
    public void LocalizedDescrFields2f_returns_localized_fields()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var entity = new TestEntity2f() { Translations = [] };
        var name = TranslatedField.CreateList((en, "en_name"), (ar, "ar_name"));
        var descr = TranslatedField.CreateList((en, "en_descr"), (ar, "ar_descr"));
        entity.AddOrUpdateTranslations(name, descr);

        // Act
        var localizedFields = entity.LocalizedDescrFields2f();

        // Assert
        Assert.Equal(2, localizedFields.Length);
        Assert.Equal(en.Id, localizedFields[0].Language);
        Assert.Equal("en_descr", localizedFields[0].Value);
        Assert.Equal(ar.Id, localizedFields[1].Language);
        Assert.Equal("ar_descr", localizedFields[1].Value);
    }

    private static void AssertTranslation2f(TestEntityTranslation2f t, Language l, string name, string descr)
    {
        Assert.Same(l.Id, t.LanguageId);
        Assert.Equal(t.Name, name);
        Assert.Equal(t.Description, descr);
    }

    private class TestEntity : Entity, ILocalizedEntity<TestEntity, TestEntityTranslation>
    {
        public ICollection<TestEntityTranslation> Translations { get; set; }
    }

    private class TestEntityTranslation : TranslationBase<TestEntity> { }

    private class TestEntity2f : Entity, ILocalizedEntity<TestEntity2f, TestEntityTranslation2f>
    {
        public ICollection<TestEntityTranslation2f> Translations { get; set; }
    }

    private class TestEntityTranslation2f : TranslationBase2f<TestEntity2f> { }
}
