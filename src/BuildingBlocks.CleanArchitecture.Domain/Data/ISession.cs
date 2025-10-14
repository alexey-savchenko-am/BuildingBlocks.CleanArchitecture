using System.Data;

namespace BuildingBlocks.CleanArchitecture.Domain.Data;

public interface ISession
{
    IDbTransaction StartTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task StoreAsync(CancellationToken ct = default);
}