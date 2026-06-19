namespace ProfAly.CMS.Infrastructure.Persistence.Seeding;

/// <summary>
/// A unit of idempotent seed work. Implementations declare their dependencies via
/// the constructor (DbContext, Identity managers, configuration, logger) and are run
/// in ascending <see cref="Order"/> by the <see cref="DatabaseInitializer"/> after
/// migrations are applied. Each seeder must be safe to run repeatedly.
/// </summary>
public interface IDataSeeder
{
    /// <summary>Lower runs first.</summary>
    int Order { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);
}
