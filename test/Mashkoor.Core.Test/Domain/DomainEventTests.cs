using Mashkoor.Core.Domain;

namespace Mashkoor.Core.Test.Domain;

public class DomainEventTests
{
    [Fact]
    public void DateOccurred_defaults_to_current_utc()
    {
        // Arrange and act
        var e = new TestEvent();

        // Assert
        Assert.True((DateTime.UtcNow - e.DateOccurred).Seconds < 5);
    }

    private class TestEvent : DomainEvent { }
}
