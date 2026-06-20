using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Web.Infrastructure;
using ProfileEntity = ProfAly.CMS.Domain.Entities.Profile;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class HomeModel : PublicPageModel
{
    public HomeModel(AppDbContext db) : base(db) { }

    public ProfileEntity? Profile { get; private set; }
    public List<Credibility> Credibility { get; private set; } = new();
    public List<StatItem> Stats { get; private set; } = new();
    public List<Book> FeaturedBooks { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();

        Profile = await Db.Profile.Include(p => p.Photo).Include(p => p.BioImage).Include(p => p.Translations).FirstOrDefaultAsync();
        Credibility = await Db.Credibility.Include(c => c.Translations).OrderBy(c => c.SortOrder).ToListAsync();
        Stats = await Db.StatItem.Include(s => s.Translations).OrderBy(s => s.SortOrder).ToListAsync();
        // Featured books first (4 on desktop). If fewer than 4 are flagged Featured,
        // top up with the next published books by CMS order so the row stays full —
        // featured items always lead, preserving the featuring/ordering logic.
        FeaturedBooks = await Db.ContentItem.OfType<Book>()
            .Where(b => b.IsPublished)
            .Include(b => b.Translations).Include(b => b.CoverImage)
            .OrderByDescending(b => b.IsFeatured).ThenBy(b => b.SortOrder)
            .Take(4).ToListAsync();
    }
}
