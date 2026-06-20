using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class ThesesModel : PublicPageModel
{
    public ThesesModel(AppDbContext db) : base(db) { }

    public List<Thesis> Items { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();
        // Newest → oldest (by publication year), then curated order.
        Items = await Db.ContentItem.OfType<Thesis>().Where(x => x.IsPublished)
            .Include(x => x.Translations)
            .OrderByDescending(x => x.PublicationYear).ThenBy(x => x.SortOrder)
            .ToListAsync();
    }
}
