using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ProfAly.CMS.Infrastructure.Persistence.Seeding;

/// <summary>
/// The database initialization pipeline (Stage 3). On startup it:
/// (1) ensures the SQLite data directory exists, (2) applies pending migrations
/// (creating the database on first run), (3) validates connectivity, and
/// (4) runs the registered seeders in order. Every step is logged. Idempotent.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly AppDbContext _context;
    private readonly IEnumerable<IDataSeeder> _seeders;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        AppDbContext context,
        IEnumerable<IDataSeeder> seeders,
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _seeders = seeders;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Database initialization starting.");

        EnsureDataDirectoryExists();
        await ApplyMigrationsAsync(cancellationToken);
        await ValidateConnectionAsync(cancellationToken);
        await RunSeedersAsync(cancellationToken);

        _logger.LogInformation("Database initialization complete.");
    }

    /// <summary>SQLite does not create the database file's parent folder; ensure it exists.</summary>
    private void EnsureDataDirectoryExists()
    {
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created database directory {Directory}.", directory);
        }
    }

    private async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<string> pending;
        try
        {
            pending = (await _context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        }
        catch
        {
            // Database/history table not present yet → Migrate will create everything.
            pending = Array.Empty<string>();
        }

        if (pending.Count > 0)
        {
            _logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", pending.Count, string.Join(", ", pending));
        }
        else
        {
            _logger.LogInformation("No pending migrations detected; ensuring database exists.");
        }

        await _context.Database.MigrateAsync(cancellationToken);
    }

    private async Task ValidateConnectionAsync(CancellationToken cancellationToken)
    {
        if (!await _context.Database.CanConnectAsync(cancellationToken))
        {
            throw new InvalidOperationException("Database connectivity validation failed after initialization.");
        }

        _logger.LogInformation("Database connectivity validated.");
    }

    private async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders.OrderBy(s => s.Order))
        {
            _logger.LogInformation("Running seeder {Seeder} (order {Order}).", seeder.GetType().Name, seeder.Order);
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
