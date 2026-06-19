using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// A YouTube video reference (doc 08 §5). Video is never stored — only the id is kept,
/// and no PDF applies.
/// </summary>
public class Video : ContentItem
{
    public Video() : base(ContentType.Video)
    {
    }

    /// <summary>11-character YouTube video id (required).</summary>
    public string YouTubeVideoId { get; set; } = string.Empty;

    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>(base.Validate());
        if (!ContentRules.IsValidYouTubeId(YouTubeVideoId))
        {
            errors.Add("A valid 11-character YouTube video id is required for a video.");
        }

        if (PdfFileId.HasValue)
        {
            errors.Add("A video cannot have an attached PDF.");
        }

        return errors;
    }
}
