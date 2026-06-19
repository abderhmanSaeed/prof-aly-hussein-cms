using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// SEO metadata for a static page, keyed by (PageKey, Culture) — doc 03 §2.5 / doc 05 §18.
/// Standalone (not a child translation); falls back to sensible defaults when empty.
/// </summary>
public class PageSeo : BaseEntity
{
    /// <summary>Page identifier, e.g. "home", "about". Unique with <see cref="Culture"/>.</summary>
    public string PageKey { get; set; } = string.Empty;

    public string Culture { get; set; } = SupportedCultures.Default;

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }
}
