using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// An editable content block for a static page (home/about/contact hero, intros),
/// doc 03 §2.5 / doc 05 §11. Keyed by <see cref="PageKey"/>.
/// </summary>
public class PageSection : AuditableEntity
{
    /// <summary>Page identifier, e.g. "home", "about", "contact". Unique.</summary>
    public string PageKey { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public ICollection<PageSectionTranslation> Translations { get; set; } = new List<PageSectionTranslation>();
}

public class PageSectionTranslation : BaseEntity, ITranslation
{
    public int PageSectionId { get; set; }

    public PageSection? PageSection { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string? Heading { get; set; }

    /// <summary>Rich-text body (sanitised before render).</summary>
    public string? Body { get; set; }
}
