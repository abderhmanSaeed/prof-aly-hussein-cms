using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProfAly.CMS.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools (<c>dotnet ef</c>) to create the
/// context for migrations without building the web host. Runtime configuration is
/// done in <see cref="DependencyInjection"/>; this only needs a provider + connection.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(
                "Data Source=App_Data/app.db",
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name))
            .Options;

        return new AppDbContext(options);
    }
}
