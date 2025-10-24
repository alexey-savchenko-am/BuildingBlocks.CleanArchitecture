
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using System.Threading;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database;

internal sealed class DatabaseInitializer
    : IDatabaseInitializer
{
    private readonly DbContext _dbContext;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(DbContext dbContext, ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Applying migrations for {DbContext}...", typeof(DbContext).Name);

        try
        {
            await _dbContext.Database.MigrateAsync(ct);
            _logger.LogInformation("✅ Migrations applied successfully for {DbContext}.", typeof(DbContext).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Migration failed for {DbContext}.", typeof(DbContext).Name);
            throw;
        }
    }
}
