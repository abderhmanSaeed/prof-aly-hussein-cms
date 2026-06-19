using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Memberships;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Membership> Items { get; private set; } = new();

    public async Task OnGetAsync() =>
        Items = await _db.Membership.Include(x => x.Translations)
            .OrderBy(x => x.Kind).ThenBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var entity = await _db.Membership.FindAsync(id);
        if (entity is not null)
        {
            _db.Membership.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, string dir)
    {
        var entity = await _db.Membership.FindAsync(id);
        if (entity is null)
        {
            return RedirectToPage();
        }

        // Reorder within the same Kind group.
        var group = await _db.Membership.Where(x => x.Kind == entity.Kind)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        var index = group.FindIndex(x => x.Id == id);
        var target = dir == "up" ? index - 1 : index + 1;
        if (target < 0 || target >= group.Count)
        {
            return RedirectToPage();
        }

        for (var i = 0; i < group.Count; i++)
        {
            group[i].SortOrder = i;
        }

        (group[index].SortOrder, group[target].SortOrder) = (group[target].SortOrder, group[index].SortOrder);
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
