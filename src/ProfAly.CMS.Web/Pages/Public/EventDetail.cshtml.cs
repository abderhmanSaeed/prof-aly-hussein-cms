using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class EventDetailModel : PublicPageModel
{
    public EventDetailModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true)]
    public string Slug { get; set; } = string.Empty;

    public Event? Event { get; private set; }
    public List<Event> Related { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadChromeAsync();

        Event = await Db.ContentItem.OfType<Event>()
            .Include(e => e.Translations)
            .Include(e => e.CoverImage)
            .Include(e => e.Images.OrderBy(i => i.SortOrder)).ThenInclude(i => i.MediaFile)
            .Where(e => e.IsPublished && e.Translations.Any(t => t.Slug == Slug))
            .FirstOrDefaultAsync();

        if (Event is null)
        {
            return NotFound();
        }

        Related = await Db.ContentItem.OfType<Event>()
            .Where(e => e.IsPublished && e.Id != Event.Id)
            .Include(e => e.Translations).Include(e => e.CoverImage)
            .OrderByDescending(e => e.EventDateUtc).ThenBy(e => e.SortOrder)
            .Take(4).ToListAsync();

        return Page();
    }
}
