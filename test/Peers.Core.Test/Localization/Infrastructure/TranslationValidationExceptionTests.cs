using Peers.Core.Localization.Infrastructure;

namespace Peers.Core.Test.Localization.Infrastructure;

public class TranslationValidationExceptionTests
{
    [Fact]
    public void Ctor_SetsMessage()
    {
        // Arrange
        var langCode = "en";
        var reason = "Some reason '{0}'";

        // Act
        var ex = new TranslationValidationException(reason, langCode);

        // Assert
        Assert.Equal($"Some reason '{langCode}'", ex.Message);
    }

    [Fact]
    public void Ctor_SetsProperties()
    {
        // Arrange
        var langCode = "en";
        var reason = "Some reason '{0}'";

        // Act
        var ex = new TranslationValidationException(reason, langCode);

        // Assert
        Assert.Equal(reason, ex.Formatted);
        Assert.Equal(langCode, ex.LangCode);
    }
}
