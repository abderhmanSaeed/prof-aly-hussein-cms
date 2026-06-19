using System.Text.RegularExpressions;

namespace ProfAly.CMS.Domain.Common;

/// <summary>
/// Domain knowledge for content formats and ranges (doc 05): YouTube id, DOI,
/// hex colour, publication-year bounds, and allowed redirect status codes.
/// Used by entity invariants and by services to validate input consistently.
/// </summary>
public static partial class ContentRules
{
    public const int MinPublicationYear = 1900;

    /// <summary>Allowed HTTP redirect status codes (doc 05 §16).</summary>
    public static readonly IReadOnlyList<int> RedirectStatusCodes = new[] { 301, 302 };

    /// <summary>Upper bound for a publication year: next calendar year (UTC).</summary>
    public static int MaxPublicationYear() => DateTime.UtcNow.Year + 1;

    public static bool IsValidPublicationYear(int year) =>
        year >= MinPublicationYear && year <= MaxPublicationYear();

    public static bool IsValidYouTubeId(string? id) =>
        !string.IsNullOrWhiteSpace(id) && YouTubeIdRegex().IsMatch(id);

    public static bool IsValidDoi(string? doi) =>
        string.IsNullOrWhiteSpace(doi) || DoiRegex().IsMatch(doi);

    public static bool IsValidHexColor(string? color) =>
        string.IsNullOrWhiteSpace(color) || HexColorRegex().IsMatch(color);

    public static bool IsValidRedirectStatusCode(int code) =>
        RedirectStatusCodes.Contains(code);

    // 11-character YouTube video id.
    [GeneratedRegex("^[A-Za-z0-9_-]{11}$")]
    private static partial Regex YouTubeIdRegex();

    // DOI: "10.NNNN/suffix".
    [GeneratedRegex(@"^10\.\d{4,9}/\S+$")]
    private static partial Regex DoiRegex();

    // #RGB | #RRGGBB | #RRGGBBAA.
    [GeneratedRegex("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex HexColorRegex();
}
