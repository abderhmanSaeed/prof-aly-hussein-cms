using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages;

// Authorization is enforced at the area-folder level (RequireSuperAdmin) in Program.cs.
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public string CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture.NativeName;

    public string? AdminName { get; private set; }

    public int TotalMessages { get; private set; }
    public int UnreadMessages { get; private set; }
    public List<ContactMessage> LatestMessages { get; private set; } = new();

    public async Task OnGetAsync()
    {
        AdminName = User.Identity?.Name;
        CurrentCulture = CultureInfo.CurrentUICulture.NativeName;

        TotalMessages = await _db.ContactMessage.CountAsync();
        UnreadMessages = await _db.ContactMessage.CountAsync(m => !m.IsRead);
        LatestMessages = await _db.ContactMessage
            .OrderByDescending(m => m.CreatedUtc).ThenByDescending(m => m.Id)
            .Take(5).ToListAsync();
    }
}
