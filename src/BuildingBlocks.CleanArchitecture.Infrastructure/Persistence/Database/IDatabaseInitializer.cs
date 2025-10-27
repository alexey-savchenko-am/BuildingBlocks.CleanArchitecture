namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database;

public interface IDatabaseInitializer
{
    Task InitializeAsync(bool recreateDatabase = false, CancellationToken ct = default);
}
