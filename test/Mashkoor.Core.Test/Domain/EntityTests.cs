using Mashkoor.Core.Data;
using Mashkoor.Core.Domain;
using Mashkoor.Core.Domain.Rules;

namespace Mashkoor.Core.Test.Domain;

public class EntityTests
{
    [Fact]
    public void UserEntity_throws_when_rule_is_broken()
    {
        // Arrange
        var entity = new TestUserEntity();

        // Act & assert
        var ex = Assert.Throws<BusinessRuleValidationException>(entity.TestFailingMutation);
        Assert.Equal(["Error1", "Error2"], ex.BrokenRule.Errors);
        var n = Environment.NewLine;
        Assert.Equal($"error title:{n}Error1{n}Error2", ex.Message);
    }

    [Fact]
    public void UserEntity_doesnt_throw_when_rule_is_not_broken()
    {
        // Arrange
        var entity = new TestUserEntity();

        // Act
        entity.TestNonFailingMutation();

        // Assert
        // No exception is thrown.
    }

    [Fact]
    public void Mutation_throws_when_rule_is_broken()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & assert
        var ex = Assert.Throws<BusinessRuleValidationException>(entity.TestFailingMutation);
        Assert.Equal(["Error1", "Error2"], ex.BrokenRule.Errors);
        var n = Environment.NewLine;
        Assert.Equal($"error title:{n}Error1{n}Error2", ex.Message);
    }

    [Fact]
    public void Mutation_doesnt_throw_when_rule_is_not_broken()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.TestNonFailingMutation();

        // Assert
        // No exception is thrown.
    }

    [Fact]
    public void AddEvent_adds_event()
    {
        // Arrange
        var entity = new TestEntity();
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
        var entity = new TestEntity();
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
        var entity = new TestEntity();
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
        var entity = new TestEntity();

        // Act
        var events = entity.GetEvents();

        // Assert
        Assert.Empty(events);
    }

    private class TestEntity : Entity
    {
        public void TestFailingMutation() => CheckRule(new TestRule(true));
        public void TestNonFailingMutation() => CheckRule(new TestRule(false));
        public void CallAddEvent(DomainEvent @event) => AddEvent(@event);
    }

    private class TestUserEntity : IdentityUserBase
    {
        public void TestFailingMutation() => CheckRule(new TestRule(true));
        public void TestNonFailingMutation() => CheckRule(new TestRule(false));
    }

    private class TestEvent : DomainEvent { }

    private class TestRule : BusinessRule
    {
        private readonly bool _fail;

        public override string ErrorTitle => "error title";

        public TestRule(bool fail) => _fail = fail;

        public override bool IsBroken()
        {
            if (_fail)
            {
                Append("Error1");
                Append("Error2");
                return true;
            }

            return false;
        }
    }
}
