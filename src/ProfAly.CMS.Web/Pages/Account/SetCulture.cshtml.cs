using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Web.Pages.Account;

/// <summary>Sets the request-culture cookie (ar/en/fr) and returns to the caller.</summary>
[AllowAnonymous]
public class SetCultureModel : PageModel
{
    public IActionResult OnGet(string culture, string? returnUrl = null)
    {
        if (SupportedCultures.IsSupported(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/",
                });
        }

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "~/Admin" : returnUrl);
    }
}
