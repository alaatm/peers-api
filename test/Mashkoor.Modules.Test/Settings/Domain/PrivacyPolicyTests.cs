using Mashkoor.Modules.I18n.Domain;
using Mashkoor.Modules.Settings.Domain;

namespace Mashkoor.Modules.Test.Settings.Domain;

public class PrivacyPolicyTests
{
    [Fact]
    public void Create_creates_instance()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;

        var title = "title ";
        var arabicTitle = "arabicTitle ";
        var body = "body ";
        var arabicBody = "arabicBody ";
        var effectiveDate = DateTime.UtcNow.ToDateOnly();

        // Act
        var policy = PrivacyPolicy.Create(
            TranslatedField.CreateList((en, title), (ar, arabicTitle)),
            TranslatedField.CreateList((en, body), (ar, arabicBody)),
            effectiveDate);

        // Assert
        Assert.Equal(2, policy.Translations.Count);

        Assert.Same(en.Id, policy.Translations.ElementAt(0).LanguageId);
        Assert.Equal(title.Trim(), policy.Translations.ElementAt(0).Name);
        Assert.Same(ar.Id, policy.Translations.ElementAt(1).LanguageId);
        Assert.Equal(arabicTitle.Trim(), policy.Translations.ElementAt(1).Name);

        Assert.Same(en.Id, policy.Translations.ElementAt(0).LanguageId);
        Assert.Equal(body.Trim(), policy.Translations.ElementAt(0).Description);
        Assert.Same(ar.Id, policy.Translations.ElementAt(1).LanguageId);
        Assert.Equal(arabicBody.Trim(), policy.Translations.ElementAt(1).Description);

        Assert.Equal(effectiveDate, policy.EffectiveDate);
    }

    [Fact]
    public void Create_removes_newline_from_body_html()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;

        var body = "\n a\rb\nc\r\nd\ne\rf \r\n";

        // Act
        var terms = PrivacyPolicy.Create(
            TranslatedField.CreateList((en, "x"), (ar, "x")),
            TranslatedField.CreateList((en, body), (ar, body)),
            DateTime.UtcNow.ToDateOnly());

        // Assert
        Assert.Equal("abcdef", terms.Translations.ElementAt(0).Description);
        Assert.Equal("abcdef", terms.Translations.ElementAt(1).Description);
    }

    [Fact]
    public void Update_updates_instance()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;

        var title = "title";
        var arabicTitle = "arabicTitle";
        var body = "body";
        var arabicBody = "arabicBody";
        var effectiveDate = DateTime.UtcNow.ToDateOnly();

        var policy = PrivacyPolicy.Create(
            TranslatedField.CreateList((en, title), (ar, arabicTitle)),
            TranslatedField.CreateList((en, body), (ar, arabicBody)),
            DateTime.UtcNow.AddDays(-10).ToDateOnly());

        // Act
        policy.Update(
            TranslatedField.CreateList((en, $" new {title} "), (ar, $" new {arabicTitle} ")),
            TranslatedField.CreateList((en, $" new {body} "), (ar, $" new {arabicBody} ")),
            effectiveDate);

        Assert.Same(en.Id, policy.Translations.ElementAt(0).LanguageId);
        Assert.Equal($"new {title}", policy.Translations.ElementAt(0).Name);
        Assert.Same(ar.Id, policy.Translations.ElementAt(1).LanguageId);
        Assert.Equal($"new {arabicTitle}", policy.Translations.ElementAt(1).Name);

        Assert.Same(en.Id, policy.Translations.ElementAt(0).LanguageId);
        Assert.Equal($"new {body}", policy.Translations.ElementAt(0).Description);
        Assert.Same(ar.Id, policy.Translations.ElementAt(1).LanguageId);
        Assert.Equal($"new {arabicBody}", policy.Translations.ElementAt(1).Description);

        Assert.Equal(effectiveDate, policy.EffectiveDate);
    }

    [Fact]
    public void Update_adds_extra_language_translations_if_present()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var ru = new Language { Name = "Russian", Id = "ru" };

        var policy = PrivacyPolicy.Create(
            TranslatedField.CreateList((en, "title"), (ar, "arabicTitle")),
            TranslatedField.CreateList((en, "body"), (ar, "arabicBody")),
            DateTime.UtcNow.ToDateOnly());

        // Act
        policy.Update(
            TranslatedField.CreateList((en, "title"), (ar, "arabicTitle"), (ru, "russianTitle")),
            TranslatedField.CreateList((en, "body"), (ar, "arabicBody"), (ru, "russianBody")),
            DateTime.UtcNow.ToDateOnly());

        Assert.Equal(3, policy.Translations.Count);
        Assert.Same(ru.Id, policy.Translations.ElementAt(2).LanguageId);
        Assert.Equal("russianTitle", policy.Translations.ElementAt(2).Name);
        Assert.Equal("russianBody", policy.Translations.ElementAt(2).Description);
    }

    [Fact]
    public void Update_removes_newline_from_body_html()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;

        var policy = PrivacyPolicy.Create(
            TranslatedField.CreateList((en, "title"), (ar, "arabicTitle")),
            TranslatedField.CreateList((en, "body"), (ar, "arabicBody")),
            DateTime.UtcNow.ToDateOnly());

        var body = "\n a\rb\nc\r\nd\ne\rf \r\n";

        // Act
        policy.Update(
            TranslatedField.CreateList((en, "title"), (ar, "arabicTitle")),
            TranslatedField.CreateList((en, body), (ar, body)),
            DateTime.UtcNow.ToDateOnly());

        // Assert
        Assert.Equal("abcdef", policy.Translations.ElementAt(0).Description);
        Assert.Equal("abcdef", policy.Translations.ElementAt(1).Description);
    }
}
