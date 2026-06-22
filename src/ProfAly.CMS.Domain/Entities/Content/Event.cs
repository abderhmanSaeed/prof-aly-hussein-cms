using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// A public event / happening (doc 76 — Events module). Reuses shared base fields:
/// <see cref="ContentItem.EventDateUtc"/> is the event date, <see cref="ContentItem.ExternalUrl"/>
/// is the external event link, and <see cref="ContentItem.CoverImage"/> is the lead image.
/// The event location is translatable (<see cref="ContentItemTranslation.Location"/>) and
/// the photo gallery is the shared <see cref="ContentItem.Images"/> collection.
/// </summary>
public class Event : ContentItem
{
    public Event() : base(ContentType.Event)
    {
    }

    /// <summary>
    /// Optional 11-character YouTube video id for an event recording/teaser. Stored exactly
    /// like <see cref="Video.YouTubeVideoId"/> (id only — never uploaded); the thumbnail and
    /// embed URLs are derived from it at render time via <see cref="Common.YouTube"/>. Kept in
    /// its own TPH column (<c>EventVideoYouTubeId</c>) so the existing Video column is never
    /// touched. doc 90.
    /// </summary>
    public string? VideoYouTubeId { get; set; }
}
