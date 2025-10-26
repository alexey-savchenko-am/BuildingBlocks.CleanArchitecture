using BuildingBlocks.CleanArchitecture.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence;

public sealed class Session
    : ISession
    , IAsyncDisposable
{
    private readonly DbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public Session(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task StartAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("Transaction already started.");

        _transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            if (_transaction is null)
                throw new InvalidOperationException("Transaction not started.");

            await _dbContext.SaveChangesAsync(ct);
            await _transaction!.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        });
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task StoreAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }

    public ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            return _transaction.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
