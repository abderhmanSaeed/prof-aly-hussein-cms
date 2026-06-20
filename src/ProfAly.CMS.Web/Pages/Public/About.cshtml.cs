using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Web.Infrastructure;
using ProfileEntity = ProfAly.CMS.Domain.Entities.Profile;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class AboutModel : PublicPageModel
{
    public AboutModel(AppDbContext db) : base(db) { }

    public ProfileEntity? Profile { get; private set; }
    public List<Qualification> Qualifications { get; private set; } = new();
    public List<Skill> Skills { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();
        Profile = await Db.Profile.Include(p => p.Photo).Include(p => p.AboutImage).Include(p => p.Translations).FirstOrDefaultAsync();
        Qualifications = await Db.Qualification.Include(q => q.Translations).OrderBy(q => q.SortOrder).ToListAsync();
        Skills = await Db.Skill.Include(s => s.Translations).OrderBy(s => s.SortOrder).ToListAsync();
    }
}
