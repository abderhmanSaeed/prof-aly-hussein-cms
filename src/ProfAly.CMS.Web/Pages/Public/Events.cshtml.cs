using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class EventsModel : PublicPageModel
{
    private const int PageSize = 12;

    public EventsModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNum { get; set; } = 1;

    public List<Event> Featured { get; private set; } = new();
    public List<Event> Items { get; private set; } = new();
    public int TotalPages { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();

        var query = Db.ContentItem.OfType<Event>().Where(e => e.IsPublished);
        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (PageNum < 1) { PageNum = 1; }

        if (PageNum == 1)
        {
            Featured = await query.Where(e => e.IsFeatured)
                .Include(e => e.Translations).Include(e => e.CoverImage)
                .OrderByDescending(e => e.EventDateUtc).Take(3).ToListAsync();
        }

        Items = await query
            .Include(e => e.Translations).Include(e => e.CoverImage)
            .OrderByDescending(e => e.EventDateUtc).ThenBy(e => e.SortOrder)
            .Skip((PageNum - 1) * PageSize).Take(PageSize)
            .ToListAsync();
    }
}
