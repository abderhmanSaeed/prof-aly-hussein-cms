using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Events;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Event> Items { get; private set; } = new();

    public async Task OnGetAsync() =>
        Items = await _db.ContentItem.OfType<Event>()
            .Include(e => e.Translations).Include(e => e.CoverImage)
            .OrderBy(e => e.SortOrder).ThenBy(e => e.Id).ToListAsync();

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var entity = await _db.ContentItem.FindAsync(id);
        if (entity is Event)
        {
            _db.ContentItem.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public Task<IActionResult> OnPostTogglePublishAsync(int id) => MutateAsync(id, e => e.IsPublished = !e.IsPublished);

    public Task<IActionResult> OnPostToggleFeatureAsync(int id) => MutateAsync(id, e => e.IsFeatured = !e.IsFeatured);

    public async Task<IActionResult> OnPostMoveAsync(int id, string dir)
    {
        var items = await _db.ContentItem.OfType<Event>().OrderBy(e => e.SortOrder).ThenBy(e => e.Id).ToListAsync();
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

    private async Task<IActionResult> MutateAsync(int id, Action<Event> mutate)
    {
        var entity = await _db.ContentItem.FindAsync(id);
        if (entity is Event ev)
        {
            mutate(ev);
            ev.ModifiedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
