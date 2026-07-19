namespace ProfAly.CMS.Application.Abstractions;

/// <summary>
/// Creates and manages safe, point-in-time copies of the SQLite database (Database
/// Safety Layer). Backups are written to <c>App_Data/Backups</c> and are produced via
/// the SQLite online-backup API so they are consistent even while the database is in
/// use (WAL).
/// </summary>
public interface IDatabaseBackupService
{
    /// <summary>
    /// Creates a timestamped, verified backup if the database file exists.
    /// </summary>
    /// <param name="reason">Short tag included in the file name (e.g. "startup", "pre-import", "manual").</param>
    /// <returns>The backup file path, or <c>null</c> if no database file exists yet (first run) or backups are disabled.</returns>
    Task<string?> CreateBackupAsync(string reason, CancellationToken cancellationToken = default);

    /// <summary>Lists existing backups (newest first) with parsed metadata.</summary>
    IReadOnlyList<BackupInfo> ListBackups();

    /// <summary>
    /// Verifies that a backup file is a structurally valid SQLite database
    /// (runs <c>PRAGMA integrity_check</c>). <paramref name="fileName"/> is the bare
    /// file name of a file inside the backups directory (no path traversal allowed).
    /// </summary>
    Task<bool> VerifyBackupAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores the live database from the named backup. A safety backup of the current
    /// database is taken first (reason "pre-restore"), the backup is verified, then it
    /// replaces the live database file. Returns the outcome.
    /// </summary>
    Task<BackupRestoreResult> RestoreAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the absolute path of a backup file for download, or <c>null</c> if the
    /// name is invalid, escapes the backups directory, or does not exist.
    /// </summary>
    string? ResolveBackupPathForDownload(string fileName);
}

/// <summary>Metadata for a single backup file.</summary>
public sealed record BackupInfo(string FileName, DateTime CreatedUtc, long SizeBytes, string Reason);

/// <summary>Outcome of a restore operation.</summary>
public sealed record BackupRestoreResult(bool Success, string Message, string? SafetyBackupPath = null)
{
    public static BackupRestoreResult Ok(string? safety) => new(true, "Restore completed successfully.", safety);
    public static BackupRestoreResult Fail(string message) => new(false, message);
}
