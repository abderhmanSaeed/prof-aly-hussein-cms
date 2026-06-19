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
        Items = await Db.ContentItem.OfType<Thesis>().Where(x => x.IsPublished)
            .Include(x => x.Translations)
            .OrderBy(x => x.SortOrder).ThenByDescending(x => x.PublicationYear)
            .ToListAsync();
    }
}
