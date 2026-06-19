using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// Enrichment material (doc 08 §7): like a Resource but with richer in-page body
/// content; file- or link-backed.
/// </summary>
public class EnrichmentItem : ContentItem
{
    public EnrichmentItem() : base(ContentType.EnrichmentItem)
    {
    }

    /// <summary>Resource-type label (non-translatable).</summary>
    public string? ResourceType { get; set; }
}
