using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// Metadata for a file stored on disk (doc 03 §2.6 / doc 09). The bytes live under
/// the uploads root with a GUID stored name; the database holds only this record.
/// </summary>
public class MediaFile : BaseEntity
{
    /// <summary>Unique GUID-based name on disk.</summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>The uploader's original (sanitised) name, used for display/download.</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Path under the uploads root (POSIX separators), e.g. "images/2026/06/{guid}.webp".</summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>MIME content type.</summary>
    public string ContentType { get; set; } = string.Empty;

    public MediaKind MediaKind { get; set; }

    public long SizeBytes { get; set; }

    /// <summary>Pixel dimensions for images; null for PDFs.</summary>
    public int? Width { get; set; }

    public int? Height { get; set; }

    /// <summary>Accessibility text (images).</summary>
    public string? AltText { get; set; }

    public DateTime CreatedUtc { get; set; }
}
