using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>Append-only, page-level view counter (doc 03 §2.9 / doc 05 §17).</summary>
public class PageView : BaseEntity
{
    public string Path { get; set; } = string.Empty;

    public string Culture { get; set; } = SupportedCultures.Default;

    public DateTime CreatedUtc { get; set; }
}
