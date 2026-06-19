namespace ProfAly.CMS.Application.Abstractions;

/// <summary>
/// Creates safe, point-in-time copies of the SQLite database (Database Safety Layer).
/// Backups are written to <c>App_Data/Backups</c> and are produced via the SQLite
/// online-backup API so they are consistent even while the database is in use (WAL).
/// </summary>
public interface IDatabaseBackupService
{
    /// <summary>
    /// Creates a timestamped backup if the database file exists.
    /// </summary>
    /// <param name="reason">Short tag included in the file name (e.g. "startup", "pre-import").</param>
    /// <returns>The backup file path, or <c>null</c> if no database file exists yet (first run).</returns>
    Task<string?> CreateBackupAsync(string reason, CancellationToken cancellationToken = default);
}
