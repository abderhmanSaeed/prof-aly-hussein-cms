using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Messages;

public class ViewModel : PageModel
{
    private readonly AppDbContext _db;
    public ViewModel(AppDbContext db) => _db = db;

    public ContactMessage? Message { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Message = await _db.ContactMessage.FirstOrDefaultAsync(m => m.Id == id);
        if (Message is null)
        {
            return NotFound();
        }

        // Opening a message marks it read automatically.
        if (!Message.IsRead)
        {
            Message.IsRead = true;
            await _db.SaveChangesAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostMarkUnreadAsync(int id)
    {
        var msg = await _db.ContactMessage.FindAsync(id);
        if (msg is not null)
        {
            msg.IsRead = false;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var msg = await _db.ContactMessage.FindAsync(id);
        if (msg is not null)
        {
            _db.ContactMessage.Remove(msg);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage("Index");
    }
}
