namespace ProfAly.CMS.Domain.Enums;

/// <summary>Kind of stored media file (doc 05 §14 / doc 09).</summary>
public enum MediaKind
{
    Image = 1,
    Pdf = 2,
    Thumbnail = 3,

    /// <summary>
    /// Downloadable document: PDF, Word (.doc/.docx) or PowerPoint (.ppt/.pptx).
    /// Used by Enrichment Items, which accept richer document formats than the
    /// PDF-only fields on other content types.
    /// </summary>
    Document = 4,
}
