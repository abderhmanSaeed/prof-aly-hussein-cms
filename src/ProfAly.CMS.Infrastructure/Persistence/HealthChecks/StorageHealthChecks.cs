using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProfAly.CMS.Infrastructure.Persistence.HealthChecks;

/// <summary>
/// Shared probe used by the folder health checks: confirms a directory exists (creating
/// it if necessary) and is writable by round-tripping a tiny temp file.
/// </summary>
internal static class FolderProbe
{
    public static HealthCheckResult Check(string label, string path)
    {
        try
        {
            Directory.CreateDirectory(path);

            var probe = Path.Combine(path, $".healthcheck-{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);

            return HealthCheckResult.Healthy($"{label} folder is accessible and writable ({path}).");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{label} folder is not accessible ({path}).", ex);
        }
    }
}

/// <summary>Verifies the media upload root (<c>FileStorage:RootPath</c>) is accessible and writable.</summary>
public sealed class UploadFolderHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;

    public UploadFolderHealthCheck(IConfiguration config) => _config = config;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(_config["FileStorage:RootPath"] ?? "App_Data/uploads");
        return Task.FromResult(FolderProbe.Check("Upload", root));
    }
}

/// <summary>Verifies the database backup folder (<c>App_Data/Backups</c>) is accessible and writable.</summary>
public sealed class BackupFolderHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public BackupFolderHealthCheck(AppDbContext context) => _context = context;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var connectionString = _context.Database.GetConnectionString();
        var dataSource = string.IsNullOrWhiteSpace(connectionString)
            ? null
            : new SqliteConnectionStringBuilder(connectionString).DataSource;

        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            return Task.FromResult(HealthCheckResult.Healthy("No file-backed database; backup folder not applicable."));
        }

        var backupsDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(dataSource))!, "Backups");
        return Task.FromResult(FolderProbe.Check("Backup", backupsDir));
    }
}
