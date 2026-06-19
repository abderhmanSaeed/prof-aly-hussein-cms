using System.Text.RegularExpressions;

namespace ProfAly.CMS.Domain.Common;

/// <summary>
/// YouTube URL helpers (doc 08 §5). Videos are never uploaded — only the 11-character
/// video id is stored; thumbnail and embed URLs are derived from it at render time.
/// </summary>
public static partial class YouTube
{
    /// <summary>
    /// Extracts the 11-character video id from a YouTube URL (watch, youtu.be, embed,
    /// shorts, /v/) or from a bare id. Returns false if no valid id can be found.
    /// </summary>
    public static bool TryGetVideoId(string? input, out string videoId)
    {
        videoId = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var s = input.Trim();

        // Already a bare id.
        if (ContentRules.IsValidYouTubeId(s))
        {
            videoId = s;
            return true;
        }

        var match = UrlRegex().Match(s);
        if (match.Success && ContentRules.IsValidYouTubeId(match.Groups["id"].Value))
        {
            videoId = match.Groups["id"].Value;
            return true;
        }

        return false;
    }

    /// <summary>Standard thumbnail URL for a video id.</summary>
    public static string ThumbnailUrl(string videoId) => $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";

    /// <summary>Privacy-friendly embed URL for the iframe player.</summary>
    public static string EmbedUrl(string videoId) => $"https://www.youtube-nocookie.com/embed/{videoId}";

    /// <summary>Canonical watch URL.</summary>
    public static string WatchUrl(string videoId) => $"https://www.youtube.com/watch?v={videoId}";

    // Matches the id after watch?v= / youtu.be/ / embed/ / shorts/ / v/ (with optional preceding query args).
    [GeneratedRegex(@"(?:youtu\.be/|youtube\.com/(?:watch\?(?:.*&)?v=|embed/|shorts/|v/))(?<id>[A-Za-z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();
}
