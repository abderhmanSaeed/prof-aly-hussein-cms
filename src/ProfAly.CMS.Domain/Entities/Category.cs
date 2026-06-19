using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities.Content;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>Cross-cutting topical taxonomy, many-to-many with content (doc 03 §2.3 / doc 08 §10).</summary>
public class Category : AuditableEntity
{
    public int SortOrder { get; set; }

    public ICollection<CategoryTranslation> Translations { get; set; } = new List<CategoryTranslation>();

    public ICollection<ContentItemCategory> ContentLinks { get; set; } = new List<ContentItemCategory>();
}

/// <summary>Per-culture category name + slug (doc 03 §2.3).</summary>
public class CategoryTranslation : BaseEntity, ITranslation
{
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string Name { get; set; } = string.Empty;

    /// <summary>Unique per culture.</summary>
    public string Slug { get; set; } = string.Empty;
}

/// <summary>Join entity for the ContentItem ↔ Category many-to-many (composite key, doc 03 §2.3).</summary>
public class ContentItemCategory
{
    public int ContentItemId { get; set; }

    public ContentItem? ContentItem { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}
