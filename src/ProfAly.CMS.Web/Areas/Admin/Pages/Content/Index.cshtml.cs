using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Content;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string Type { get; set; } = "Book";

    public ContentType ContentType { get; private set; }
    public List<ContentItem> Items { get; private set; } = new();

    /// <summary>
    /// The content types this generic module manages. Books/Publications/Research are the
    /// academic collections; EnrichmentItem and RecommendedBook (doc 76 — Digital Resources)
    /// reuse the same TPH plumbing (cover, PDF, external URL, categories, SEO, AR/EN/FR).
    /// </summary>
    public static readonly ContentType[] Allowed =
    {
        ContentType.Book, ContentType.Publication, ContentType.ResearchPaper,
        ContentType.EnrichmentItem, ContentType.RecommendedBook,
    };

    public string TitleKey => ContentType switch
    {
        ContentType.Publication => "Publications_Title",
        ContentType.ResearchPaper => "Research_Title",
        ContentType.EnrichmentItem => "Enrichment_Title",
        ContentType.RecommendedBook => "RecommendedBooks_Title",
        _ => "Books_Title",
    };

    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryResolveType(out var ct))
        {
            return NotFound();
        }

        ContentType = ct;
        Items = await _db.ContentItem
            .Where(c => c.ContentType == ct)
            .Include(c => c.Translations)
            .Include(c => c.CoverImage)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .ToListAsync();
        return Page();
    }

    public Task<IActionResult> OnPostDeleteAsync(int id) => MutateAsync(id, e => _db.ContentItem.Remove(e));

    public Task<IActionResult> OnPostTogglePublishAsync(int id) =>
        MutateAsync(id, e => e.IsPublished = !e.IsPublished);

    public Task<IActionResult> OnPostToggleFeatureAsync(int id) =>
        MutateAsync(id, e => e.IsFeatured = !e.IsFeatured);

    public async Task<IActionResult> OnPostMoveAsync(int id, string dir)
    {
        if (!TryResolveType(out var ct))
        {
            return NotFound();
        }

        var items = await _db.ContentItem.Where(c => c.ContentType == ct)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id).ToListAsync();
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

        return RedirectToPage(new { Type });
    }

    private async Task<IActionResult> MutateAsync(int id, Action<ContentItem> mutate)
    {
        if (!TryResolveType(out _))
        {
            return NotFound();
        }

        var entity = await _db.ContentItem.FindAsync(id);
        if (entity is not null)
        {
            mutate(entity);
            if (entity.Id != 0)
            {
                entity.ModifiedUtc = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { Type });
    }

    private bool TryResolveType(out ContentType ct)
    {
        ct = default;
        return Enum.TryParse(Type, out ct) && Allowed.Contains(ct);
    }
}
