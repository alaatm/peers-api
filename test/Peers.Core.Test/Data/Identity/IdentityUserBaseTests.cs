using Peers.Core.Data;
using Peers.Core.Domain;

namespace Peers.Core.Test.Data.Identity;

public class IdentityUserBaseTests
{
    [Fact]
    public void AddEvent_adds_event()
    {
        // Arrange
        var entity = new TestUserEntity();
        var testEvent = new TestEvent();

        // Act
        entity.CallAddEvent(testEvent);

        // Assert
        Assert.Same(testEvent, Assert.Single(entity.GetEvents()));
    }

    [Fact]
    public void GetEvents_returns_and_clears_events_by_default()
    {
        // Arrange
        var entity = new TestUserEntity();
        entity.CallAddEvent(new TestEvent());
        entity.CallAddEvent(new TestEvent());

        // Act
        var events = entity.GetEvents();

        // Assert
        Assert.Equal(2, events.Length);
        Assert.Empty(entity.GetEvents());
    }

    [Fact]
    public void GetEvents_returns_but_doesnt_clears_events_when_specified()
    {
        // Arrange
        var entity = new TestUserEntity();
        entity.CallAddEvent(new TestEvent());
        entity.CallAddEvent(new TestEvent());

        // Act
        var events = entity.GetEvents(false);

        // Assert
        Assert.Equal(2, events.Length);
        Assert.Equal(2, entity.GetEvents().Length);
    }

    [Fact]
    public void GetEvents_noops_when_no_events()
    {
        // Arrange
        var entity = new TestUserEntity();

        // Act
        var events = entity.GetEvents();

        // Assert
        Assert.Empty(events);
    }

    private class TestUserEntity : IdentityUserBase
    {
        public void CallAddEvent(DomainEvent @event) => AddEvent(@event);
    }

    private class TestEvent : DomainEvent { }
}
