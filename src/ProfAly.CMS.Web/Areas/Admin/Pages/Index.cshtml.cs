using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProfAly.CMS.Web.Areas.Admin.Pages;

// Authorization is enforced at the area-folder level (RequireSuperAdmin) in Program.cs.
public class IndexModel : PageModel
{
    public string CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture.NativeName;

    public string? AdminName { get; private set; }

    public void OnGet()
    {
        AdminName = User.Identity?.Name;
        CurrentCulture = CultureInfo.CurrentUICulture.NativeName;
    }
}
