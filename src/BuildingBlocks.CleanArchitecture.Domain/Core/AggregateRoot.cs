namespace BuildingBlocks.CleanArchitecture.Domain.Core;


public abstract class GuidKeyAggregateRoot<TId>
    : AggregateRoot<TId, Guid>
    where TId : EntityId<TId, Guid>
{
    protected GuidKeyAggregateRoot(TId id) 
        : base(id)
    {
    }
}

public abstract class StringKeyAggregateRoot<TId>
    : AggregateRoot<TId, string>
    where TId : EntityId<TId, string>
{
    protected StringKeyAggregateRoot(TId id)
        : base(id)
    {
    }
}

public abstract class AggregateRoot<TId, TKey>(TId id)
    : Entity<TId, TKey>(id)
    , IAggregateRoot
    where TId : EntityId<TId, TKey>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}