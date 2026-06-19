using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities;

/// <summary>
/// Singleton (Id = 1) holding site-wide chrome and theme settings (doc 03 §2.1).
/// </summary>
public class SiteSettings : AuditableEntity
{
    /// <summary>First-visit default culture (ar/en/fr). See <see cref="SupportedCultures"/>.</summary>
    public string DefaultCulture { get; set; } = SupportedCultures.Default;

    /// <summary>First-visit default theme; the public toggle persists the visitor's choice.</summary>
    public ThemeMode DefaultTheme { get; set; } = ThemeMode.Light;

    public int? LogoMediaId { get; set; }

    public MediaFile? Logo { get; set; }

    public string? FacebookUrl { get; set; }

    public string? WhatsAppNumber { get; set; }

    /// <summary>Receives contact-form mail.</summary>
    public string ContactEmail { get; set; } = string.Empty;

    public string? PrimaryColor { get; set; }

    public string? SecondaryColor { get; set; }

    public ICollection<SiteSettingsTranslation> Translations { get; set; } = new List<SiteSettingsTranslation>();
}

/// <summary>Per-culture site chrome text (doc 03 §2.1).</summary>
public class SiteSettingsTranslation : BaseEntity, ITranslation
{
    public int SiteSettingsId { get; set; }

    public SiteSettings? SiteSettings { get; set; }

    public string Culture { get; set; } = SupportedCultures.Default;

    public string SiteTitle { get; set; } = string.Empty;

    public string? FooterText { get; set; }

    public string? Tagline { get; set; }
}
