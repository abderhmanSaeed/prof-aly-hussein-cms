using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Web.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    // Root entry point → redirect to the default culture's home.
    public IActionResult OnGet() => Redirect($"/{SupportedCultures.Default}");
}
