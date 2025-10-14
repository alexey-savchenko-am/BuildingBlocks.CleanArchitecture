using BuildingBlocks.CleanArchitecture.Domain.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.Connection;

public sealed class SqlServerConnectionFactory
    : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _connectionString = options?.Value.ConnectionString
            ?? throw new ArgumentNullException(nameof(options));
    }

    public IDbConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}