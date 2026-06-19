using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ProfAly.CMS.Infrastructure.Persistence.Seeding;

/// <summary>
/// Applies pending migrations and runs registered <see cref="IDataSeeder"/>s in order.
/// Stage 2 ships the mechanism only; with no seeders registered yet, <see cref="RunAsync"/>
/// simply migrates. The startup call that invokes this is wired in a later stage.
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

    public async Task RunAsync(bool applyMigrations = true, CancellationToken cancellationToken = default)
    {
        if (applyMigrations)
        {
            _logger.LogInformation("Applying database migrations.");
            await _context.Database.MigrateAsync(cancellationToken);
        }

        foreach (var seeder in _seeders.OrderBy(s => s.Order))
        {
            _logger.LogInformation("Running seeder {Seeder}.", seeder.GetType().Name);
            await seeder.SeedAsync(_context, cancellationToken);
        }
    }
}
