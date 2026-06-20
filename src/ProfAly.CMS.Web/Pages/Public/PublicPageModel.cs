using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Web.Infrastructure;

namespace ProfAly.CMS.Web.Pages.Public;

/// <summary>
/// Base for all public pages. Resolves the current culture and loads header/footer
/// chrome (site title, tagline, contact) from the database into ViewData.
/// All public content is read from the DB only (no static files).
/// </summary>
public abstract class PublicPageModel : PageModel
{
    protected AppDbContext Db { get; }

    protected PublicPageModel(AppDbContext db) => Db = db;

    public string Culture
    {
        get
        {
            var c = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return SupportedCultures.IsSupported(c) ? c : SupportedCultures.Default;
        }
    }

    protected async Task LoadChromeAsync(CancellationToken cancellationToken = default)
    {
        var settings = await Db.SiteSettings.Include(s => s.Translations).FirstOrDefaultAsync(cancellationToken);
        var st = Localized.Pick(settings?.Translations, Culture);
        ViewData["SiteTitle"] = st?.SiteTitle ?? "Prof. Aly Hussein";
        ViewData["Tagline"] = st?.Tagline;

        var profile = await Db.Profile.FirstOrDefaultAsync(cancellationToken);
        ViewData["ContactPhone"] = profile?.Phone;
        // CMS-managed Profile.Email is authoritative; SiteSettings.ContactEmail is only a
        // bootstrap fallback used until profile data exists (business rule — see report 66).
        ViewData["ContactEmail"] = ContactEmailOf(profile, settings);
    }

    /// <summary>Effective public contact email: the CMS-managed <see cref="Profile.Email"/> when set, else the bootstrap <see cref="SiteSettings.ContactEmail"/>.</summary>
    protected static string? ContactEmailOf(Profile? profile, SiteSettings? settings) =>
        !string.IsNullOrWhiteSpace(profile?.Email) ? profile!.Email : settings?.ContactEmail;
}
