using Peers.Core.Commands;

namespace Peers.Core.Test.Commands;

public class LocalizedCommandTests
{
    [Fact]
    public void Lang_returns_current_thread_language()
    {
        // Arrange
        var defaultCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");

        // Act
        var actual = new TestCommand().Lang;

        // Assert
        Assert.Equal("ar", actual);
        Thread.CurrentThread.CurrentCulture = defaultCulture;
    }

    private record TestCommand : LocalizedCommand;
}
