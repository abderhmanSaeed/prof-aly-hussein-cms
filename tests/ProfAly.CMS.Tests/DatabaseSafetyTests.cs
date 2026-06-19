using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Infrastructure.Persistence.Backup;
using ProfAly.CMS.Infrastructure.Persistence.Seeding.Seeders;

namespace ProfAly.CMS.Tests;

/// <summary>
/// Database Safety Layer tests — run against a throwaway temp SQLite file (never App_Data).
/// Verifies (1) automatic backups produce a file in App_Data/Backups, and (2) the static
/// content import runs exactly once (StaticContentImported marker), with no duplication.
/// </summary>
public class DatabaseSafetyTests : IDisposable
{
    private readonly string _dir;
    private readonly string _dbPath;

    public DatabaseSafetyTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "profaly-safety-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _dbPath = Path.Combine(_dir, "app.db");
    }

    private AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;
        return new AppDbContext(options);
    }

    private static ConfigurationManager Config(bool import, bool force = false)
    {
        var c = new ConfigurationManager();
        c["Seed:ImportStaticContent"] = import ? "true" : "false";
        c["Seed:ForceImport"] = force ? "true" : "false";
        return c;
    }

    [Fact]
    public async Task Backup_CreatesTimestampedCopy_InBackupsFolder()
    {
        await using (var ctx = NewContext())
        {
            await ctx.Database.MigrateAsync();
        }

        await using var ctx2 = NewContext();
        var backup = new SqliteDatabaseBackupService(ctx2, Config(import: false), NullLogger<SqliteDatabaseBackupService>.Instance);

        var path = await backup.CreateBackupAsync("test");

        Assert.NotNull(path);
        Assert.True(File.Exists(path));
        Assert.True(new FileInfo(path!).Length > 0);
        Assert.Equal(Path.Combine(_dir, "Backups"), Path.GetDirectoryName(path));
        Assert.Contains("-test", Path.GetFileName(path));
    }

    [Fact]
    public async Task Backup_ReturnsNull_WhenDatabaseFileMissing()
    {
        // Context configured but Migrate never called → no file on disk.
        await using var ctx = NewContext();
        var backup = new SqliteDatabaseBackupService(ctx, Config(import: false), NullLogger<SqliteDatabaseBackupService>.Instance);

        var path = await backup.CreateBackupAsync("startup");

        Assert.Null(path);
    }

    [Fact]
    public async Task Import_RunsExactlyOnce_AndSetsMarker()
    {
        await using (var ctx = NewContext())
        {
            await ctx.Database.MigrateAsync();
        }

        async Task<int> RunImportAndCount(bool force = false)
        {
            await using var ctx = NewContext();
            var backup = new SqliteDatabaseBackupService(ctx, Config(import: true), NullLogger<SqliteDatabaseBackupService>.Instance);
            var importer = new StaticContentImporter(ctx, Config(import: true, force: force), backup, NullLogger<StaticContentImporter>.Instance);
            await importer.SeedAsync();
            return await ctx.ContentItem.CountAsync();
        }

        var afterFirst = await RunImportAndCount();
        Assert.True(afterFirst > 0, "first import should populate content");

        // Marker set?
        await using (var ctx = NewContext())
        {
            var marker = await ctx.SystemSetting
                .FirstOrDefaultAsync(s => s.Key == SystemSettingKeys.StaticContentImported);
            Assert.NotNull(marker);
            Assert.Equal("true", marker!.Value);

            // Content present and not duplicated by the (idempotent) per-table guards.
            Assert.Equal(7, await ctx.ActivityGroup.CountAsync());
            Assert.Equal(1, await ctx.Profile.CountAsync());
        }

        // Second run must be a no-op (once-only) → counts unchanged.
        var afterSecond = await RunImportAndCount();
        Assert.Equal(afterFirst, afterSecond);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_dir, recursive: true); } catch { /* temp cleanup best-effort */ }
        GC.SuppressFinalize(this);
    }
}
