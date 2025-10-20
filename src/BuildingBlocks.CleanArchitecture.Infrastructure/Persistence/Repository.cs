using BuildingBlocks.CleanArchitecture.Domain.Core;
using BuildingBlocks.CleanArchitecture.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence;

public abstract class Repository<TAggregateRoot, TId, TKey>
    : IRepository<TAggregateRoot, TId, TKey>
    where TAggregateRoot : AggregateRoot<TId, TKey>
    where TId : EntityId<TId, TKey>
{
    protected DbContext DbContext { get; }

    protected DbSet<TAggregateRoot> Set { get; }

    public Repository(DbContext dbContext)
    {
        DbContext = dbContext;
        Set = dbContext.Set<TAggregateRoot>();
    }

    public async ValueTask<TAggregateRoot?> FindByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.FindAsync<TAggregateRoot>(id, cancellationToken);
    }

    public async Task AddAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TAggregateRoot>().AddAsync(aggregate, cancellationToken);
    }

    public async Task AddListAsync(List<TAggregateRoot> aggregateList, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TAggregateRoot>().AddRangeAsync(aggregateList, cancellationToken);
    }

    public async Task<int> RemoveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        var removedRowCount = await DbContext.Set<TAggregateRoot>()
            .Where(entity => entity.Id == aggregate.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return removedRowCount;
    }
}
