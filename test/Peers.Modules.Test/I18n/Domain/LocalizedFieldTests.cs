using Peers.Modules.I18n.Domain;

namespace Peers.Modules.Test.I18n.Domain;

public class LocalizedFieldTests
{
    [Fact]
    public void ToTranslatedField_returns_the_mapped_field()
    {
        // Arrange
        var languages = new[] { Language.En, Language.Ar };
        var lf = new LocalizedField("en", "English translation");

        // Act
        var actual = lf.ToTranslatedField(languages);

        // Assert
        Assert.Same(languages[0], actual.Language);
        Assert.Equal("English translation", actual.Value);
    }

    [Fact]
    public void ToTranslatedField_throws_when_no_language_mapping_could_be_found()
    {
        // Arrange
        var languages = new[] { Language.Ar };
        var lf = new LocalizedField("en", "English translation");

        // Act and assert
        var ex = Assert.Throws<InvalidOperationException>(() => lf.ToTranslatedField(languages));
        Assert.Equal("No language found for 'en'.", ex.Message);
    }

    [Fact]
    public void CreateList_creates_localized_fields_list()
    {
        // Arrange and act
        var actual = LocalizedField.CreateList(("en", "English translation"), ("ar", "ترجمة عربية"));

        // Assert
        Assert.Equal(2, actual.Length);
        Assert.Equal("en", actual[0].Language);
        Assert.Equal("English translation", actual[0].Value);
        Assert.Equal("ar", actual[1].Language);
        Assert.Equal("ترجمة عربية", actual[1].Value);
    }

    [Fact]
    public void ToTranslatedFields_returns_the_mapped_fields()
    {
        // Arrange
        var languages = new[] { Language.En, Language.Ar };
        var fields = new[]
        {
            new LocalizedField("en", "English translation"),
            new LocalizedField("ar", "ترجمة عربية")
        };

        // Act
        var actual = fields.ToTranslatedFields(languages);

        // Assert
        Assert.Equal(2, actual.Length);
        Assert.Same(languages[0], actual[0].Language);
        Assert.Equal("English translation", actual[0].Value);
        Assert.Same(languages[1], actual[1].Language);
        Assert.Equal("ترجمة عربية", actual[1].Value);
    }
}
