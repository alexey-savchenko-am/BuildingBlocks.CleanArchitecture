using System.Data;

namespace BuildingBlocks.CleanArchitecture.Domain.Data;

public interface IDbConnectionFactory
{
    IDbConnection GetConnection();
}
