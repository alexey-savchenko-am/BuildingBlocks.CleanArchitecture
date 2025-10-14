using BuildingBlocks.CleanArchitecture.Domain.Core;

namespace BuildingBlocks.CleanArchitecture.Domain.Data;

public interface IRepository<TAggregateRoot, TId>
    where TAggregateRoot : AggregateRoot<TId>
    where TId : EntityId<TId, Guid>
{
    ValueTask<TAggregateRoot?> FindByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default);
    Task AddListAsync(List<TAggregateRoot> aggregateList, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default);
}
