using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProfAly.CMS.Application.Abstractions;

namespace ProfAly.CMS.Infrastructure.Persistence.Backup;

/// <summary>
/// Database Safety Layer — produces consistent, timestamped copies of the SQLite
/// database into <c>App_Data/Backups</c> using the SQLite online-backup API (safe
/// even while the database is open / has an active WAL). Backups are additive; old
/// backups are pruned only when <c>Backup:KeepLast</c> is configured (&gt; 0).
/// </summary>
public sealed class SqliteDatabaseBackupService : IDatabaseBackupService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<SqliteDatabaseBackupService> _logger;

    public SqliteDatabaseBackupService(
        AppDbContext context,
        IConfiguration config,
        ILogger<SqliteDatabaseBackupService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task<string?> CreateBackupAsync(string reason, CancellationToken cancellationToken = default)
    {
        if (!_config.GetValue("Backup:Enabled", true))
        {
            return null;
        }

        var dbPath = ResolveDatabasePath();
        if (dbPath is null)
        {
            return null; // in-memory or unset
        }

        if (!File.Exists(dbPath))
        {
            _logger.LogInformation("No database file at {Path} yet; nothing to back up.", dbPath);
            return null;
        }

        var backupsDir = Path.Combine(Path.GetDirectoryName(dbPath)!, "Backups");
        Directory.CreateDirectory(backupsDir);

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var destination = Path.Combine(backupsDir, $"app-{stamp}-{Sanitize(reason)}.db");

        try
        {
            await using var source = new SqliteConnection($"Data Source={dbPath}");
            await using var target = new SqliteConnection($"Data Source={destination}");
            await source.OpenAsync(cancellationToken);
            await target.OpenAsync(cancellationToken);
            source.BackupDatabase(target); // consistent online snapshot (handles WAL)
        }
        catch (Exception ex)
        {
            // A backup failure must not crash startup; migrations are additive and the
            // importer is guarded, so continuing is safe — but log loudly.
            _logger.LogError(ex, "Database backup FAILED (reason '{Reason}'); continuing without a backup.", reason);
            return null;
        }

        var info = new FileInfo(destination);
        _logger.LogInformation("Database backup created: {Path} ({Size:N0} bytes).", destination, info.Exists ? info.Length : 0);

        PruneOldBackups(backupsDir);
        return destination;
    }

    private string? ResolveDatabasePath()
    {
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            return null;
        }

        return Path.GetFullPath(dataSource);
    }

    private void PruneOldBackups(string backupsDir)
    {
        // Off by default (0 = keep every backup → nothing is ever deleted).
        var keepLast = _config.GetValue("Backup:KeepLast", 0);
        if (keepLast <= 0)
        {
            return;
        }

        try
        {
            var stale = new DirectoryInfo(backupsDir)
                .GetFiles("app-*.db")
                .OrderByDescending(f => f.Name) // timestamped names sort chronologically
                .Skip(keepLast)
                .ToList();

            foreach (var file in stale)
            {
                file.Delete();
            }

            if (stale.Count > 0)
            {
                _logger.LogInformation("Pruned {Count} old backup(s); keeping the most recent {Keep}.", stale.Count, keepLast);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Backup pruning failed (non-fatal).");
        }
    }

    private static string Sanitize(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return "manual";
        }

        var chars = reason.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        return new string(chars).Trim('-');
    }
}
