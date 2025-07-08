using Mashkoor.Core.Queries;

namespace Mashkoor.Core.Test.Queries;

public class LocalizedQueryTests
{
    [Fact]
    public void Lang_returns_current_thread_language()
    {
        // Arrange
        var defaultCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");

        // Act
        var actual = new TestQuery().Lang;

        // Assert
        Assert.Equal("ar", actual);
        Thread.CurrentThread.CurrentCulture = defaultCulture;
    }

    private record TestQuery : LocalizedQuery;
}
