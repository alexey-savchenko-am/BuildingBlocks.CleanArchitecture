namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken ct = default);
}
