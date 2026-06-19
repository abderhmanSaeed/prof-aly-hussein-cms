using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// Per-culture text for a <see cref="ContentItem"/> (doc 03 §2.2). Carries the
/// shared translatable fields plus the type-specific translatable fields, which
/// are populated only for the relevant content type.
/// </summary>
public class ContentItemTranslation : BaseEntity, ITranslation
{
    public int ContentItemId { get; set; }

    public ContentItem? ContentItem { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Title { get; set; } = string.Empty;

    /// <summary>Lowercase, url-safe; unique per (ContentType, Culture).</summary>
    public string Slug { get; set; } = string.Empty;

    public string? Summary { get; set; }

    /// <summary>Full rich-text body (sanitised before render).</summary>
    public string? Body { get; set; }

    // --- Type-specific translatable fields (doc 03 §2.2) ---

    /// <summary>Publication / ResearchPaper: journal/venue (+ issue).</summary>
    public string? Journal { get; set; }

    /// <summary>Publication / ResearchPaper: author list.</summary>
    public string? Authors { get; set; }

    /// <summary>Book: publisher (name + place).</summary>
    public string? Publisher { get; set; }

    /// <summary>Book: authorship role (e.g. "Sole author", "With dept. members").</summary>
    public string? AuthorshipRole { get; set; }

    /// <summary>Thesis: the student/researcher name.</summary>
    public string? ResearcherName { get; set; }

    // --- SEO ---

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }
}
