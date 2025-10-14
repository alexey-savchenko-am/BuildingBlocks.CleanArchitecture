namespace BuildingBlocks.CleanArchitecture.Domain.Data;

public interface IDbInitializer
{
    Task<bool> InitializeAsync(bool recreateDatabase);
}
