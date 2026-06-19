namespace ProfAly.CMS.Domain.Common;

/// <summary>
/// Single source of truth for the platform's cultures, shared by routing,
/// resource localization, and database CHECK constraints (docs 03 §4, 10).
/// Arabic is the primary/default culture and renders RTL.
/// French is first-class from day one; its content may be empty (fallback applies).
/// </summary>
public static class SupportedCultures
{
    public const string Arabic = "ar";
    public const string English = "en";
    public const string French = "fr";

    /// <summary>Default culture when none is specified (Arabic).</summary>
    public const string Default = Arabic;

    /// <summary>All supported culture codes, in display order.</summary>
    public static readonly IReadOnlyList<string> All = new[] { Arabic, English, French };

    public static bool IsSupported(string? culture) =>
        culture is not null && All.Contains(culture, StringComparer.OrdinalIgnoreCase);

    public static bool IsRightToLeft(string? culture) =>
        string.Equals(culture, Arabic, StringComparison.OrdinalIgnoreCase);
}
