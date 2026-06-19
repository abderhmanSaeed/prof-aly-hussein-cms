using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class VideosModel : PublicPageModel
{
    private const int PageSize = 12;

    public VideosModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNum { get; set; } = 1;

    public List<Video> Featured { get; private set; } = new();
    public List<Video> Items { get; private set; } = new();
    public int TotalPages { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();

        var query = Db.ContentItem.OfType<Video>().Where(v => v.IsPublished);
        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (PageNum < 1) { PageNum = 1; }

        if (PageNum == 1)
        {
            Featured = await query.Where(v => v.IsFeatured)
                .Include(v => v.Translations)
                .OrderBy(v => v.SortOrder).Take(3).ToListAsync();
        }

        Items = await query
            .Include(v => v.Translations)
            .OrderBy(v => v.SortOrder).ThenByDescending(v => v.EventDateUtc)
            .Skip((PageNum - 1) * PageSize).Take(PageSize)
            .ToListAsync();
    }
}
