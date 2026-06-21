using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class EnrichmentModel : PublicPageModel
{
    private const int PageSize = 12;

    public EnrichmentModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Query { get; set; }

    public List<EnrichmentItem> Featured { get; private set; } = new();
    public List<EnrichmentItem> Items { get; private set; } = new();
    public int TotalPages { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();

        var query = Db.ContentItem.OfType<EnrichmentItem>().Where(e => e.IsPublished);

        var q = Query?.Trim();
        if (!string.IsNullOrEmpty(q))
        {
            var like = $"%{q}%";
            query = query.Where(e => e.Translations.Any(t =>
                EF.Functions.Like(t.Title, like)
                || (t.Summary != null && EF.Functions.Like(t.Summary, like))));
        }

        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (PageNum < 1) { PageNum = 1; }

        if (PageNum == 1 && string.IsNullOrEmpty(q))
        {
            Featured = await query.Where(e => e.IsFeatured)
                .Include(e => e.Translations).Include(e => e.CoverImage)
                .OrderBy(e => e.SortOrder).Take(3).ToListAsync();
        }

        Items = await query
            .Include(e => e.Translations).Include(e => e.CoverImage)
            .OrderBy(e => e.SortOrder).ThenBy(e => e.Id)
            .Skip((PageNum - 1) * PageSize).Take(PageSize)
            .ToListAsync();
    }
}
