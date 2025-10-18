using System.Data;

namespace BuildingBlocks.CleanArchitecture.Domain.Data;

public interface ISession
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    Task StartAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default);
}