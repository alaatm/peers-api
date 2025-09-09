using Peers.Core.Domain.Rules;

namespace Peers.Core.Domain;

/// <summary>
/// Base contract for all domain entities.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// The entity id.
    /// </summary>
    int Id { get; }
    /// <summary>
    /// Returns and, optionally, clears all domain events for this entity.
    /// </summary>
    /// <returns></returns>
    DomainEvent[] GetEvents(bool clear);
}

/// <summary>
/// Base class for all domain entities
/// </summary>
public abstract class Entity : IEntity
{
    private List<DomainEvent>? _events;
    // For testing which rules were checked.
    internal List<IBusinessRule> CheckedRules { get; } = [];

    /// <summary>
    /// The entity id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Adds the specified event to the list of domain events for this entity.
    /// </summary>
    /// <param name="event">The domain event.</param>
    protected void AddEvent(DomainEvent @event)
        => (_events ??= []).Add(@event);

    /// <summary>
    /// Checks that the specified business rule is not broken
    /// and throws <see cref="BusinessRuleValidationException"/> if it is.
    /// </summary>
    /// <param name="rule">The rule to check.</param>
    protected void CheckRule(IBusinessRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        CheckedRules.Add(rule);
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }

    /// <summary>
    /// Returns and, optionally, clears all domain events for this entity.
    /// </summary>
    /// <returns></returns>
    public DomainEvent[] GetEvents(bool clear = true)
    {
        var events = Array.Empty<DomainEvent>();

        if (_events is not null)
        {
            events = [.. _events];
            if (clear)
            {
                _events.Clear();
            }
        }

        return events;
    }
}
