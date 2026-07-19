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
/// even while the database is open / has an active WAL). Every backup is verified
/// (<c>PRAGMA integrity_check</c>) immediately after creation. Backups are additive;
/// old backups are pruned only when <c>Backup:KeepLast</c> is configured (&gt; 0).
///
/// The file-name convention is <c>app-yyyyMMdd-HHmmss-{reason}.db</c>, which both sorts
/// chronologically and carries the timestamp + reason metadata surfaced in the admin UI.
/// </summary>
public sealed class SqliteDatabaseBackupService : IDatabaseBackupService
{
    private const string FilePrefix = "app-";
    private const string FileExtension = ".db";

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

        var backupsDir = GetBackupsDirectory(dbPath);
        Directory.CreateDirectory(backupsDir);

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var destination = Path.Combine(backupsDir, $"{FilePrefix}{stamp}-{Sanitize(reason)}{FileExtension}");

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
        var verified = await IntegrityCheckAsync(destination, cancellationToken);
        if (!verified)
        {
            _logger.LogError("Database backup {Path} FAILED integrity verification; the file is retained for inspection.", destination);
        }
        else
        {
            _logger.LogInformation(
                "Database backup created and verified: {Path} ({Size:N0} bytes).",
                destination,
                info.Exists ? info.Length : 0);
        }

        PruneOldBackups(backupsDir);
        return destination;
    }

    public IReadOnlyList<BackupInfo> ListBackups()
    {
        var dbPath = ResolveDatabasePath();
        if (dbPath is null)
        {
            return Array.Empty<BackupInfo>();
        }

        var backupsDir = GetBackupsDirectory(dbPath);
        if (!Directory.Exists(backupsDir))
        {
            return Array.Empty<BackupInfo>();
        }

        return new DirectoryInfo(backupsDir)
            .GetFiles($"{FilePrefix}*{FileExtension}")
            .OrderByDescending(f => f.Name) // timestamped names sort chronologically
            .Select(ToBackupInfo)
            .ToList();
    }

    public Task<bool> VerifyBackupAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var path = ResolveBackupPathForDownload(fileName);
        return path is null ? Task.FromResult(false) : IntegrityCheckAsync(path, cancellationToken);
    }

    public async Task<BackupRestoreResult> RestoreAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var dbPath = ResolveDatabasePath();
        if (dbPath is null)
        {
            return BackupRestoreResult.Fail("The live database path could not be resolved (in-memory or unset).");
        }

        var source = ResolveBackupPathForDownload(fileName);
        if (source is null)
        {
            return BackupRestoreResult.Fail("Backup file not found.");
        }

        // Never restore a corrupt backup over a working database.
        if (!await IntegrityCheckAsync(source, cancellationToken))
        {
            return BackupRestoreResult.Fail("The selected backup failed integrity verification and was NOT restored.");
        }

        // Always snapshot the current state first so a restore is itself reversible.
        var safety = await CreateBackupAsync("pre-restore", cancellationToken);

        try
        {
            // Release EF/ADO connection pool handles so the file can be replaced on Windows.
            SqliteConnection.ClearAllPools();

            // WAL/SHM sidecars belong to the OLD database; discard them so SQLite does not
            // replay stale journal pages on top of the restored file.
            DeleteIfExists(dbPath + "-wal");
            DeleteIfExists(dbPath + "-shm");

            File.Copy(source, dbPath, overwrite: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restore from {Source} FAILED.", source);
            return BackupRestoreResult.Fail($"Restore failed: {ex.Message}. A pre-restore safety backup was taken.");
        }

        _logger.LogWarning("Database RESTORED from backup {Source}. Pre-restore safety backup: {Safety}.", Path.GetFileName(source), safety);
        return BackupRestoreResult.Ok(safety is null ? null : Path.GetFileName(safety));
    }

    public string? ResolveBackupPathForDownload(string fileName)
    {
        // Accept only a bare file name that matches our naming convention — reject any
        // path separators or traversal so a request can never escape the backups folder.
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var bare = Path.GetFileName(fileName);
        if (!string.Equals(bare, fileName, StringComparison.Ordinal))
        {
            return null; // contained a directory component
        }

        if (!bare.StartsWith(FilePrefix, StringComparison.Ordinal) ||
            !bare.EndsWith(FileExtension, StringComparison.Ordinal))
        {
            return null;
        }

        var dbPath = ResolveDatabasePath();
        if (dbPath is null)
        {
            return null;
        }

        var backupsDir = GetBackupsDirectory(dbPath);
        var full = Path.GetFullPath(Path.Combine(backupsDir, bare));

        // Defence in depth: ensure the resolved path really is inside the backups folder.
        var normalizedDir = Path.GetFullPath(backupsDir) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(normalizedDir, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
        {
            return null;
        }

        return full;
    }

    private async Task<bool> IntegrityCheckAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqliteConnection($"Data Source={path};Mode=ReadOnly");
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            var result = (await command.ExecuteScalarAsync(cancellationToken))?.ToString();
            return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Integrity check could not be run against {Path}.", path);
            return false;
        }
    }

    private static BackupInfo ToBackupInfo(FileInfo file)
    {
        // Expected: app-yyyyMMdd-HHmmss-{reason}.db
        var name = Path.GetFileNameWithoutExtension(file.Name);
        var body = name.StartsWith(FilePrefix, StringComparison.Ordinal) ? name[FilePrefix.Length..] : name;

        DateTime created = file.CreationTimeUtc;
        string reason = "unknown";

        var parts = body.Split('-', 3);
        if (parts.Length >= 2 &&
            DateTime.TryParseExact(
                $"{parts[0]}-{parts[1]}",
                "yyyyMMdd-HHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            created = parsed;
            reason = parts.Length == 3 ? parts[2] : "manual";
        }

        return new BackupInfo(file.Name, created, file.Length, reason);
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

    private static string GetBackupsDirectory(string dbPath) =>
        Path.Combine(Path.GetDirectoryName(dbPath)!, "Backups");

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
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
                .GetFiles($"{FilePrefix}*{FileExtension}")
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
