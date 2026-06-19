using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Skills;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Skill> Items { get; private set; } = new();

    public async Task OnGetAsync() =>
        Items = await _db.Skill.Include(x => x.Translations).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var entity = await _db.Skill.FindAsync(id);
        if (entity is not null)
        {
            _db.Skill.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, string dir)
    {
        var items = await _db.Skill.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        var index = items.FindIndex(x => x.Id == id);
        if (index < 0)
        {
            return RedirectToPage();
        }

        var target = dir == "up" ? index - 1 : index + 1;
        if (target < 0 || target >= items.Count)
        {
            return RedirectToPage();
        }

        for (var i = 0; i < items.Count; i++)
        {
            items[i].SortOrder = i;
        }

        (items[index].SortOrder, items[target].SortOrder) = (items[target].SortOrder, items[index].SortOrder);
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
