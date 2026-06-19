namespace ProfAly.CMS.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seed-infrastructure contract (Stage 2 scaffolding only — no seed data yet).
/// Concrete seeders (SiteSettings/Profile/reference data and the content import)
/// are added in Stage 3+. Implementations are discovered and run in <see cref="Order"/>
/// by the database initializer at startup.
/// </summary>
public interface IDataSeeder
{
    /// <summary>Lower runs first. Lets dependent seeders order themselves.</summary>
    int Order { get; }

    Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default);
}
