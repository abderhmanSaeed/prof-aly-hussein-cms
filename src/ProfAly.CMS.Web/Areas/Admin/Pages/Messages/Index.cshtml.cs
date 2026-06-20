using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Messages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    /// <summary>Inbox filter: all | unread | read.</summary>
    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "all";

    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Query { get; set; }

    public List<ContactMessage> Items { get; private set; } = new();
    public int TotalCount { get; private set; }
    public int UnreadCount { get; private set; }

    public async Task OnGetAsync()
    {
        TotalCount = await _db.ContactMessage.CountAsync();
        UnreadCount = await _db.ContactMessage.CountAsync(m => !m.IsRead);

        var query = _db.ContactMessage.AsQueryable();
        query = Filter?.ToLowerInvariant() switch
        {
            "unread" => query.Where(m => !m.IsRead),
            "read" => query.Where(m => m.IsRead),
            _ => query,
        };

        var q = Query?.Trim();
        if (!string.IsNullOrEmpty(q))
        {
            var like = $"%{q}%";
            query = query.Where(m =>
                EF.Functions.Like(m.Name, like)
                || EF.Functions.Like(m.Email, like)
                || (m.Subject != null && EF.Functions.Like(m.Subject, like)));
        }

        Items = await query.OrderByDescending(m => m.CreatedUtc).ThenByDescending(m => m.Id).ToListAsync();
    }

    public async Task<IActionResult> OnPostMarkReadAsync(int id, bool read)
    {
        var msg = await _db.ContactMessage.FindAsync(id);
        if (msg is not null)
        {
            msg.IsRead = read;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { Filter, q = Query });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var msg = await _db.ContactMessage.FindAsync(id);
        if (msg is not null)
        {
            _db.ContactMessage.Remove(msg);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { Filter, q = Query });
    }
}
