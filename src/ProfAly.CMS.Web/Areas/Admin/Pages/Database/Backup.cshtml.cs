using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProfAly.CMS.Application.Abstractions;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Database;

/// <summary>
/// Admin → System → Database: manual backup creation, download, restore, and the
/// backup list with metadata (timestamp, reason, size). The admin area is already
/// gated by <c>Policies.RequireSuperAdmin</c>; anti-forgery is enforced on all posts.
/// </summary>
public class BackupModel : PageModel
{
    private readonly IDatabaseBackupService _backups;
    private readonly ILogger<BackupModel> _logger;

    public BackupModel(IDatabaseBackupService backups, ILogger<BackupModel> logger)
    {
        _backups = backups;
        _logger = logger;
    }

    public IReadOnlyList<BackupInfo> Backups { get; private set; } = Array.Empty<BackupInfo>();

    public long TotalBytes { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public bool StatusIsError { get; set; }

    public void OnGet() => Load();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var path = await _backups.CreateBackupAsync("manual", HttpContext.RequestAborted);
        if (path is null)
        {
            StatusMessage = "No backup was created (no database file exists yet, or backups are disabled).";
            StatusIsError = true;
        }
        else
        {
            _logger.LogInformation("Manual backup created by admin: {File}.", Path.GetFileName(path));
            StatusMessage = $"Backup created and verified: {Path.GetFileName(path)}.";
            StatusIsError = false;
        }

        return RedirectToPage();
    }

    public IActionResult OnGetDownload(string fileName)
    {
        var path = _backups.ResolveBackupPathForDownload(fileName);
        if (path is null)
        {
            StatusMessage = "Backup file not found.";
            StatusIsError = true;
            return RedirectToPage();
        }

        return PhysicalFile(path, "application/octet-stream", Path.GetFileName(path));
    }

    public async Task<IActionResult> OnPostRestoreAsync(string fileName)
    {
        var result = await _backups.RestoreAsync(fileName, HttpContext.RequestAborted);
        StatusMessage = result.Success
            ? $"{result.Message} A pre-restore safety backup was saved ({result.SafetyBackupPath})."
            : result.Message;
        StatusIsError = !result.Success;
        return RedirectToPage();
    }

    private void Load()
    {
        Backups = _backups.ListBackups();
        TotalBytes = Backups.Sum(b => b.SizeBytes);
    }
}
