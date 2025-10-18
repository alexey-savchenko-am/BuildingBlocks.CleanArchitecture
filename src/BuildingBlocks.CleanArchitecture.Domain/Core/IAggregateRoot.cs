namespace BuildingBlocks.CleanArchitecture.Domain.Core;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}