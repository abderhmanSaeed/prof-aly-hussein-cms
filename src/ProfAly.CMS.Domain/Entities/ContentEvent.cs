using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>An append-only content interaction event (doc 03 §2.9 / doc 05 §17).</summary>
public class ContentEvent : BaseEntity
{
    public int ContentItemId { get; set; }

    public ContentItem? ContentItem { get; set; }

    public ContentEventType EventType { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public DateTime CreatedUtc { get; set; }
}
