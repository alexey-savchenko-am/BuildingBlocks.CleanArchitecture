using BuildingBlocks.CleanArchitecture.Domain.Data;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.Connection;

public sealed class NpgConnectionFactory
    : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _connectionString = options?.Value.ConnectionString
            ?? throw new ArgumentNullException(nameof(options));
    }

    public IDbConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}