using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class PublicationsModel : PublicPageModel
{
    public PublicationsModel(AppDbContext db) : base(db) { }

    public List<Publication> Items { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();
        Items = await Db.ContentItem.OfType<Publication>().Where(x => x.IsPublished)
            .Include(x => x.Translations).Include(x => x.PdfFile)
            .OrderByDescending(x => x.PublicationYear).ThenBy(x => x.SortOrder)
            .ToListAsync();
    }
}
