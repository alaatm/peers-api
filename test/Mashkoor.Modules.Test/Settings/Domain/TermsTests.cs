using Mashkoor.Modules.I18n.Domain;
using Mashkoor.Modules.Settings.Domain;

namespace Mashkoor.Modules.Test.Settings.Domain;

public class TermsTests
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

        // Act
        var terms = Terms.Create(
            TranslatedField.CreateList((en, title), (ar, arabicTitle)),
            TranslatedField.CreateList((en, body), (ar, arabicBody)));

        // Assert
        Assert.Equal(2, terms.Translations.Count);

        Assert.Same(en.Id, terms.Translations.ElementAt(0).LanguageId);
        Assert.Equal(title.Trim(), terms.Translations.ElementAt(0).Name);
        Assert.Same(ar.Id, terms.Translations.ElementAt(1).LanguageId);
        Assert.Equal(arabicTitle.Trim(), terms.Translations.ElementAt(1).Name);

        Assert.Same(en.Id, terms.Translations.ElementAt(0).LanguageId);
        Assert.Equal(body.Trim(), terms.Translations.ElementAt(0).Description);
        Assert.Same(ar.Id, terms.Translations.ElementAt(1).LanguageId);
        Assert.Equal(arabicBody.Trim(), terms.Translations.ElementAt(1).Description);
    }

    [Fact]
    public void Create_removes_newline_from_body_html()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;

        var body = "\n a\rb\nc\r\nd\ne\rf \r\n";

        // Act
        var terms = Terms.Create(
            TranslatedField.CreateList((en, "x"), (ar, "x")),
            TranslatedField.CreateList((en, body), (ar, body)));

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
        var terms = Terms.Create(
            TranslatedField.CreateList((en, title), (ar, arabicTitle)),
            TranslatedField.CreateList((en, body), (ar, arabicBody)));

        // Act
        terms.Update(
            TranslatedField.CreateList((en, $" new {title} "), (ar, $" new {arabicTitle} ")),
            TranslatedField.CreateList((en, $" new {body} "), (ar, $" new {arabicBody} ")));

        Assert.Same(en.Id, terms.Translations.ElementAt(0).LanguageId);
        Assert.Equal($"new {title}", terms.Translations.ElementAt(0).Name);
        Assert.Same(ar.Id, terms.Translations.ElementAt(1).LanguageId);
        Assert.Equal($"new {arabicTitle}", terms.Translations.ElementAt(1).Name);

        Assert.Same(en.Id, terms.Translations.ElementAt(0).LanguageId);
        Assert.Equal($"new {body}", terms.Translations.ElementAt(0).Description);
        Assert.Same(ar.Id, terms.Translations.ElementAt(1).LanguageId);
        Assert.Equal($"new {arabicBody}", terms.Translations.ElementAt(1).Description);
    }

    [Fact]
    public void Update_adds_extra_language_translations_if_present()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;
        var ru = new Language { Name = "Russian", Id = "ru" };

        var terms = Terms.Create(
            TranslatedField.CreateList((en, "title"), (ar, "arabicTitle")),
            TranslatedField.CreateList((en, "body"), (ar, "arabicBody")));

        // Act
        terms.Update(
            TranslatedField.CreateList((en, "title"), (ar, "arabicTitle"), (ru, "russianTitle")),
            TranslatedField.CreateList((en, "body"), (ar, "arabicBody"), (ru, "russianBody")));

        Assert.Equal(3, terms.Translations.Count);
        Assert.Same(ru.Id, terms.Translations.ElementAt(2).LanguageId);
        Assert.Equal("russianTitle", terms.Translations.ElementAt(2).Name);
        Assert.Equal("russianBody", terms.Translations.ElementAt(2).Description);
    }

    [Fact]
    public void Update_removes_newline_from_body_html()
    {
        // Arrange
        var en = Language.En;
        var ar = Language.Ar;

        var terms = Terms.Create(
            TranslatedField.CreateList((en, "title"), (ar, "arabicTitle")),
            TranslatedField.CreateList((en, "body"), (ar, "arabicBody")));

        var body = "\n a\rb\nc\r\nd\ne\rf \r\n";

        // Act
        terms.Update(
            TranslatedField.CreateList((en, "title"), (ar, "arabicTitle")),
            TranslatedField.CreateList((en, body), (ar, body)));

        // Assert
        Assert.Equal("abcdef", terms.Translations.ElementAt(0).Description);
        Assert.Equal("abcdef", terms.Translations.ElementAt(1).Description);
    }
}
