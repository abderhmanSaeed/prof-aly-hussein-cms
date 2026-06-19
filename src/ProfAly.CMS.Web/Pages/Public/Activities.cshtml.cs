using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class ActivitiesModel : PublicPageModel
{
    public ActivitiesModel(AppDbContext db) : base(db) { }

    public List<ActivityGroup> Groups { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();
        Groups = await Db.ActivityGroup
            .Include(g => g.Translations)
            .Include(g => g.Activities).ThenInclude(a => a.Translations)
            .OrderBy(g => g.SortOrder).ToListAsync();
    }
}
