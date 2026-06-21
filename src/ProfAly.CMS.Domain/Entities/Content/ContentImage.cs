using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// One image in a content item's gallery (doc 76 — Events module). Generic by design
/// (not Event-specific) so any <see cref="ContentItem"/> can carry an ordered set of
/// gallery images in addition to its single <see cref="ContentItem.CoverImage"/>.
/// The bytes live in <see cref="MediaFile"/>; this row is the ordering/caption link.
/// </summary>
public class ContentImage : BaseEntity
{
    public int ContentItemId { get; set; }

    public ContentItem? ContentItem { get; set; }

    public int MediaFileId { get; set; }

    public MediaFile? MediaFile { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Optional caption (non-translatable; falls back to media alt text).</summary>
    public string? Caption { get; set; }
}
