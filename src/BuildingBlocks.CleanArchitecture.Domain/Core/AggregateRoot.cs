namespace BuildingBlocks.CleanArchitecture.Domain.Core;

public abstract class AggregateRoot<TId>
    : Entity<TId>
    where TId : EntityId<TId, Guid>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}