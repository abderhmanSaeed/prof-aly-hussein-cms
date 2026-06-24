namespace ProfAly.CMS.Infrastructure.Storage;

/// <summary>
/// Bound from the "FileStorage" configuration section (doc 09). Size limits are
/// surfaced here for the media service (Stage 5/6); the storage layer itself
/// only uses <see cref="RootPath"/>.
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Uploads root, outside the web content root. May be absolute or app-relative.</summary>
    public string RootPath { get; set; } = "App_Data/uploads";

    public long MaxImageBytes { get; set; } = 5 * 1024 * 1024;   // 5 MB

    public long MaxPdfBytes { get; set; } = 25 * 1024 * 1024;    // 25 MB

    /// <summary>Limit for Enrichment document uploads (PDF / Word / PowerPoint).</summary>
    public long MaxDocumentBytes { get; set; } = 50 * 1024 * 1024;  // 50 MB
}
