using Mashkoor.Modules.I18n.Domain;

namespace Mashkoor.Modules.Test.I18n.Domain;

public class TranslatedFieldTests
{
    [Fact]
    public void CreateList_creates_translated_fields_list()
    {
        // Arrange and act
        var en = Language.En;
        var ar = Language.Ar;
        var actual = TranslatedField.CreateList((en, "English translation"), (ar, "ترجمة عربية"));

        // Assert
        Assert.Equal(2, actual.Length);
        Assert.Same(en, actual[0].Language);
        Assert.Equal("English translation", actual[0].Value);
        Assert.Same(ar, actual[1].Language);
        Assert.Equal("ترجمة عربية", actual[1].Value);
    }
}
