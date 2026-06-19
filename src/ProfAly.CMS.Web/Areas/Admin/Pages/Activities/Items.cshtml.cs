using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Activities;

public class ItemsModel : PageModel
{
    private readonly AppDbContext _db;
    public ItemsModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int GroupId { get; set; }

    public string? GroupName { get; private set; }
    public List<Activity> Items { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var group = await _db.ActivityGroup.Include(g => g.Translations).FirstOrDefaultAsync(g => g.Id == GroupId);
        if (group is null)
        {
            return NotFound();
        }

        GroupName = (group.Translations.FirstOrDefault(t => t.Culture == SupportedCultures.Default) ?? group.Translations.FirstOrDefault())?.Name;
        Items = await _db.Activity.Where(a => a.ActivityGroupId == GroupId).Include(a => a.Translations)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Id).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var entity = await _db.Activity.FindAsync(id);
        if (entity is not null)
        {
            _db.Activity.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { GroupId });
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, string dir)
    {
        var items = await _db.Activity.Where(a => a.ActivityGroupId == GroupId)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Id).ToListAsync();
        var index = items.FindIndex(x => x.Id == id);
        if (index >= 0)
        {
            var target = dir == "up" ? index - 1 : index + 1;
            if (target >= 0 && target < items.Count)
            {
                for (var i = 0; i < items.Count; i++)
                {
                    items[i].SortOrder = i;
                }

                (items[index].SortOrder, items[target].SortOrder) = (items[target].SortOrder, items[index].SortOrder);
                await _db.SaveChangesAsync();
            }
        }

        return RedirectToPage(new { GroupId });
    }
}
