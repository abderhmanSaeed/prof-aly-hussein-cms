using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace ProfAly.CMS.Web.Infrastructure;

/// <summary>
/// Renders profile biography fields that may hold EITHER legacy plain text
/// (paragraphs separated by a blank line) OR rich HTML produced by the admin
/// rich-text editor. Legacy plain text keeps its original paragraph rendering;
/// HTML content is emitted as-is. This keeps existing stored content displaying
/// exactly as before while supporting newly authored formatted bios.
/// </summary>
public static class BioContent
{
    private static readonly Regex HtmlTag = new(
        @"<(p|br|div|span|strong|b|em|i|u|ol|ul|li|h[1-6]|a)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>True when the value appears to contain editor-produced HTML markup.</summary>
    public static bool LooksLikeHtml(string? value) =>
        !string.IsNullOrWhiteSpace(value) && HtmlTag.IsMatch(value);

    /// <summary>
    /// Full biography as block HTML: raw when rich, otherwise the legacy
    /// blank-line-separated paragraphs (HTML-encoded) — identical to the
    /// previous rendering for plain-text content.
    /// </summary>
    public static IHtmlContent Render(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return HtmlString.Empty;
        }

        if (LooksLikeHtml(value))
        {
            return new HtmlString(value);
        }

        return new HtmlString(Paragraphs(value.Split("\n\n", StringSplitOptions.RemoveEmptyEntries)));
    }

    /// <summary>
    /// Short teaser for the home page: the first paragraph only, preserving the
    /// previous behaviour for plain text and falling back to the short bio.
    /// </summary>
    public static IHtmlContent RenderTeaser(string? fullBio, string? shortBio)
    {
        if (!string.IsNullOrWhiteSpace(fullBio))
        {
            if (LooksLikeHtml(fullBio))
            {
                var m = Regex.Match(fullBio, @"<p\b[^>]*>.*?</p>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                return new HtmlString(m.Success ? m.Value : fullBio);
            }

            var first = fullBio.Split("\n\n", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return first is null ? HtmlString.Empty : new HtmlString(Paragraphs(new[] { first }));
        }

        if (string.IsNullOrWhiteSpace(shortBio))
        {
            return HtmlString.Empty;
        }

        return LooksLikeHtml(shortBio)
            ? new HtmlString(shortBio)
            : new HtmlString(Paragraphs(new[] { shortBio }));
    }

    /// <summary>
    /// Inline rendering for single-line contexts (e.g. the home hero "intro line"),
    /// where the value is placed inside an existing &lt;p&gt; element. Plain text is
    /// HTML-encoded as before; rich content keeps its inline formatting but has its
    /// block wrappers (&lt;p&gt;/&lt;div&gt;) flattened so no invalid nested blocks appear.
    /// </summary>
    public static IHtmlContent RenderInline(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return HtmlString.Empty;
        }

        if (!LooksLikeHtml(value))
        {
            return new HtmlString(WebUtility.HtmlEncode(value));
        }

        var inline = Regex.Replace(value, @"</?(p|div)\b[^>]*>", " ", RegexOptions.IgnoreCase);
        return new HtmlString(Regex.Replace(inline, @"\s+", " ").Trim());
    }

    /// <summary>Tag-stripped, whitespace-collapsed plain text for meta descriptions/attributes.</summary>
    public static string? PlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !LooksLikeHtml(value))
        {
            return value;
        }

        var text = WebUtility.HtmlDecode(Regex.Replace(value, "<[^>]+>", " "));
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static string Paragraphs(IEnumerable<string> parts)
    {
        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            sb.Append("<p>").Append(WebUtility.HtmlEncode(part.Trim())).Append("</p>");
        }

        return sb.ToString();
    }
}
