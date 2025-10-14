using BuildingBlocks.CleanArchitecture.Domain.Data;
using BuildingBlocks.CleanArchitecture.Domain.Output;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence;

public sealed class Session
    : ISession
{
    private readonly DbContext _dbContext;

    public Session(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IDbTransaction StartTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        IDbContextTransaction transaction = _dbContext.Database.BeginTransaction(isolationLevel);
        return transaction.GetDbTransaction();
    }

    public async Task<Result<TResult>> ExecuteWithinTransaction<TResult>(
        Func<Task<Result<TResult>>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        var result = await strategy.ExecuteAsync(async () =>
        {
            using var transaction = _dbContext.Database.BeginTransaction(isolationLevel);

            try
            {
                var result = await action();

                if (result.IsFailure)
                {
                    await transaction.RollbackAsync(ct);
                }
                else
                {
                    await _dbContext.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<TResult>.Failure(new Error("ExecuteWithinTransaction", ex.Message));
            }
        });

        return result;
    }

    public Task<Result<TResult>> ExecuteWithinReadCommitedTransaction<TResult>(Func<Task<Result<TResult>>> action, CancellationToken ct = default)
        => ExecuteWithinTransaction(action, IsolationLevel.ReadCommitted, ct);

    public Task<Result<TResult>> ExecuteWithinRepeatableReadTransaction<TResult>(Func<Task<Result<TResult>>> action, CancellationToken ct = default)
        => ExecuteWithinTransaction(action, IsolationLevel.RepeatableRead, ct);

    public Task StoreAsync(CancellationToken ct = default)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}
