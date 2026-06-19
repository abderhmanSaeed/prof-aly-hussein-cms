namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// Key/value store for system-level flags and markers that must survive restarts —
/// e.g. <see cref="SystemSettingKeys.StaticContentImported"/>. Distinct from
/// <see cref="SiteSettings"/> (editorial/site configuration); this holds operational state.
/// </summary>
public class SystemSetting
{
    /// <summary>Primary key — a stable, code-defined key (see <see cref="SystemSettingKeys"/>).</summary>
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public DateTime UpdatedUtc { get; set; }
}

/// <summary>Well-known <see cref="SystemSetting.Key"/> values.</summary>
public static class SystemSettingKeys
{
    /// <summary>Set to "true" after the one-time static-content import has completed.</summary>
    public const string StaticContentImported = "StaticContentImported";
}
