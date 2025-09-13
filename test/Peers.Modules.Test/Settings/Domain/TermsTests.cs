using Peers.Core.Localization;
using Peers.Modules.Settings.Domain;

namespace Peers.Modules.Test.Settings.Domain;

public class TermsTests
{
    [Fact]
    public void Create_creates_instance()
    {
        // Arrange
        var title = "title ";
        var arabicTitle = "arabicTitle ";
        var body = "body ";
        var arabicBody = "arabicBody ";

        // Act
        var terms = Terms.Create(
            [
                TermsTr.Dto.Create(Lang.EnLangCode, title, body),
                TermsTr.Dto.Create(Lang.ArLangCode, arabicTitle, arabicBody),
            ]);

        // Assert
        Assert.Equal(2, terms.Translations.Count);

        var en = terms.Translations.Single(p => p.LangCode == Lang.EnLangCode);
        Assert.Equal(title.Trim(), en.Title);
        Assert.Equal(body.Trim(), en.Body);

        var ar = terms.Translations.Single(p => p.LangCode == Lang.ArLangCode);
        Assert.Equal(arabicTitle.Trim(), ar.Title);
        Assert.Equal(arabicBody.Trim(), ar.Body);
    }

    [Fact]
    public void Update_updates_instance()
    {
        // Arrange
        var title = "title";
        var arabicTitle = "arabicTitle";
        var body = "body";
        var arabicBody = "arabicBody";

        var terms = Terms.Create(
            [
                TermsTr.Dto.Create(Lang.EnLangCode, title, body),
                TermsTr.Dto.Create(Lang.ArLangCode, arabicTitle, arabicBody),
            ]);

        // Act
        terms.Update(
            [
                TermsTr.Dto.Create(Lang.EnLangCode, $" new {title} ", $" new {body} "),
                TermsTr.Dto.Create(Lang.ArLangCode, $" new {arabicTitle} ", $" new {arabicBody} "),
            ]);

        Assert.Equal(2, terms.Translations.Count);

        var en = terms.Translations.Single(p => p.LangCode == Lang.EnLangCode);
        Assert.Equal($"new {title}", en.Title);
        Assert.Equal($"new {body}", en.Body);

        var ar = terms.Translations.Single(p => p.LangCode == Lang.ArLangCode);
        Assert.Equal($"new {arabicTitle}", ar.Title);
        Assert.Equal($"new {arabicBody}", ar.Body);
    }
}
