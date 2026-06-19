using System.Text;

namespace ProfAly.CMS.Web.Infrastructure;

/// <summary>
/// Generates URL-safe slugs (doc 11 §2). Keeps Unicode letters/digits (so Arabic
/// slugs are preserved), lowercases, and collapses other characters to hyphens.
/// </summary>
public static class SlugHelper
{
    public static string Slugify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        var previousDash = false;
        foreach (var ch in input.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                previousDash = false;
            }
            else if (!previousDash && builder.Length > 0)
            {
                builder.Append('-');
                previousDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }
}
