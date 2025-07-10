using Mashkoor.Modules.I18n.Domain;

namespace Mashkoor.Modules.Test.I18n.Domain;

public class LanguageTests
{
    [Fact]
    public void SupportedLanguages_returns_all_supported_languages()
    {
        // Arrange and act
        var actual = Language.SupportedLanguages;

        // Assert
        Assert.Equal(3, actual.Length);
        Assert.Equal(Language.Ar.Name, actual[0].Name);
        Assert.Equal(Language.Ar.Id, actual[0].Id);
        Assert.Equal(Language.Ar.Dir, actual[0].Dir);
        Assert.Equal(Language.En.Name, actual[1].Name);
        Assert.Equal(Language.En.Id, actual[1].Id);
        Assert.Equal(Language.En.Dir, actual[1].Dir);
        Assert.Equal(Language.Ru.Name, actual[2].Name);
        Assert.Equal(Language.Ru.Id, actual[2].Id);
        Assert.Equal(Language.Ru.Dir, actual[2].Dir);
    }

    [Theory]
    [MemberData(nameof(HasError_returns_whether_input_has_error_TestData))]
    public void HasError_returns_whether_input_has_error(Language[] languages, LocalizedField[] field, bool expectedResult, string expectedError, string expectedArgs)
    {
        // Arrange and act
        var actualResult = Language.HasError(languages, field, out var actualError, out var actualArgs);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        Assert.Equal(expectedError, actualError);
        Assert.Equal(expectedArgs, actualArgs);
    }

    public static TheoryData<Language[], LocalizedField[], bool, string, string> HasError_returns_whether_input_has_error_TestData() => new()
    {
        { new[] { Language.Ar, Language.En }, LocalizedField.CreateList(("ar", "عربي")), true, "No translation was provided for the following language(s): {0}. Field name: '{1}'.", "en" },
        { new[] { Language.Ar }, LocalizedField.CreateList(("ar", "عربي"), ("ru", "русский")), true, "Translation was provided for the following non-supported language(s): {0}. Field name: '{1}'.", "ru" },
        { new[] { Language.Ar }, LocalizedField.CreateList(("ar", "عربي")), false, null, null },
    };
}
