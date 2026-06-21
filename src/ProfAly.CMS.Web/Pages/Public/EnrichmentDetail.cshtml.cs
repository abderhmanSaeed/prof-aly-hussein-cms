using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class EnrichmentDetailModel : PublicPageModel
{
    public EnrichmentDetailModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true)]
    public string Slug { get; set; } = string.Empty;

    public EnrichmentItem? Item { get; private set; }
    public List<EnrichmentItem> Related { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadChromeAsync();

        Item = await Db.ContentItem.OfType<EnrichmentItem>()
            .Include(e => e.Translations)
            .Include(e => e.CoverImage)
            .Include(e => e.PdfFile)
            .Where(e => e.IsPublished && e.Translations.Any(t => t.Slug == Slug))
            .FirstOrDefaultAsync();

        if (Item is null)
        {
            return NotFound();
        }

        Related = await Db.ContentItem.OfType<EnrichmentItem>()
            .Where(e => e.IsPublished && e.Id != Item.Id)
            .Include(e => e.Translations).Include(e => e.CoverImage)
            .OrderByDescending(e => e.IsFeatured).ThenBy(e => e.SortOrder)
            .Take(4).ToListAsync();

        return Page();
    }
}
