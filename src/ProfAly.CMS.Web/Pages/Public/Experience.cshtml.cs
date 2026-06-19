using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class ExperienceModel : PublicPageModel
{
    public ExperienceModel(AppDbContext db) : base(db) { }

    public List<ExperienceEntry> Entries { get; private set; } = new();
    public List<Membership> Societies { get; private set; } = new();
    public List<Membership> Boards { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();
        Entries = await Db.ExperienceEntry.Include(e => e.Translations).OrderBy(e => e.SortOrder).ToListAsync();
        var memberships = await Db.Membership.Include(m => m.Translations).OrderBy(m => m.SortOrder).ToListAsync();
        Societies = memberships.Where(m => m.Kind == MembershipKind.Society).ToList();
        Boards = memberships.Where(m => m.Kind == MembershipKind.Board).ToList();
    }
}
