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
}
