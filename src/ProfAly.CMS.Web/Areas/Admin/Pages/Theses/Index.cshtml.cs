using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Theses;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public RelationshipType? Relationship { get; set; }

    [BindProperty(SupportsGet = true)]
    public DegreeLevel? Degree { get; set; }

    public List<Thesis> Items { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var query = _db.ContentItem.OfType<Thesis>().Include(t => t.Translations).AsQueryable();
        if (Relationship is { } rel)
        {
            query = query.Where(t => t.RelationshipType == rel);
        }
        if (Degree is { } deg)
        {
            query = query.Where(t => t.DegreeLevel == deg);
        }

        Items = await query.OrderBy(t => t.SortOrder).ThenBy(t => t.Id).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var entity = await _db.ContentItem.FindAsync(id);
        if (entity is Thesis)
        {
            _db.ContentItem.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTogglePublishAsync(int id)
    {
        var entity = await _db.ContentItem.FindAsync(id);
        if (entity is Thesis)
        {
            entity.IsPublished = !entity.IsPublished;
            entity.ModifiedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, string dir)
    {
        var items = await _db.ContentItem.OfType<Thesis>().OrderBy(t => t.SortOrder).ThenBy(t => t.Id).ToListAsync();
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
