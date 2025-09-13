using Peers.Modules.I18n.Domain;

namespace Peers.Modules.Test.I18n.Domain;

public class LanguageTests
{
    [Fact]
    public void SupportedLanguages_returns_all_supported_languages()
    {
        // Arrange and act
        var actual = Language.SupportedLanguages;

        // Assert
        Assert.Equal(2, actual.Length);
        Assert.Equal(Language.Ar.Name, actual[0].Name);
        Assert.Equal(Language.Ar.Id, actual[0].Id);
        Assert.Equal(Language.Ar.Dir, actual[0].Dir);
        Assert.Equal(Language.En.Name, actual[1].Name);
        Assert.Equal(Language.En.Id, actual[1].Id);
        Assert.Equal(Language.En.Dir, actual[1].Dir);
    }
}
