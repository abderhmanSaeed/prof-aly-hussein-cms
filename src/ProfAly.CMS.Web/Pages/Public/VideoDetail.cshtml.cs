using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class VideoDetailModel : PublicPageModel
{
    public VideoDetailModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true)]
    public string Slug { get; set; } = string.Empty;

    public Video? Video { get; private set; }
    public List<Video> Related { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadChromeAsync();

        Video = await Db.ContentItem.OfType<Video>()
            .Include(v => v.Translations)
            .Where(v => v.IsPublished && v.Translations.Any(t => t.Slug == Slug))
            .FirstOrDefaultAsync();

        if (Video is null)
        {
            return NotFound();
        }

        Related = await Db.ContentItem.OfType<Video>()
            .Where(v => v.IsPublished && v.Id != Video.Id)
            .Include(v => v.Translations)
            .OrderByDescending(v => v.IsFeatured).ThenBy(v => v.SortOrder)
            .Take(4).ToListAsync();

        return Page();
    }
}
