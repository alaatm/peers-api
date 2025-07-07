using Mashkoor.Core.Localization;

namespace Mashkoor.Core.Test.Localization;

public class LangTests
{
    [Fact]
    public void GetCurrentLanguage_returns_current_thread_language()
    {
        // Arrange
        var defaultCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");

        // Act
        var actual = Lang.GetCurrent();

        // Assert
        Assert.Equal("ar", actual);
        Thread.CurrentThread.CurrentCulture = defaultCulture;
    }

    [Fact]
    public void GetCurrentLanguage_returns_default_EN_language_when_current_thread_language_is_not_supported()
    {
        // Arrange
        var defaultCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");

        // Act
        var actual = Lang.GetCurrent();

        // Assert
        Assert.Equal("en", actual);
        Thread.CurrentThread.CurrentCulture = defaultCulture;
    }

    [Theory]
    [InlineData(null, "en")]
    [InlineData("en", "en")]
    [InlineData("ar", "ar")]
    [InlineData("ru", "ru")]
    [InlineData("fr", "en")]
    public void GetOrDefault_returns_requested_language_if_found_or_default(string requestedLang, string expectedLang)
    {
        // Act
        var actual = Lang.GetOrDefault(requestedLang);

        // Assert
        Assert.Equal(expectedLang, actual);
    }
}
