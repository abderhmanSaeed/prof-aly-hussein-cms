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

    // Digital Resources & Events homepage sections (doc 76).
    public List<Video> LatestVideos { get; private set; } = new();
    public List<EnrichmentItem> LatestEnrichment { get; private set; } = new();
    public List<RecommendedBook> RecommendedBooks { get; private set; } = new();
    public List<Event> FeaturedEvents { get; private set; } = new();

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

        LatestVideos = await Db.ContentItem.OfType<Video>()
            .Where(v => v.IsPublished)
            .Include(v => v.Translations)
            .OrderByDescending(v => v.IsFeatured).ThenBy(v => v.SortOrder).ThenByDescending(v => v.EventDateUtc)
            .Take(4).ToListAsync();

        LatestEnrichment = await Db.ContentItem.OfType<EnrichmentItem>()
            .Where(e => e.IsPublished)
            .Include(e => e.Translations).Include(e => e.CoverImage)
            .OrderByDescending(e => e.IsFeatured).ThenBy(e => e.SortOrder)
            .Take(4).ToListAsync();

        RecommendedBooks = await Db.ContentItem.OfType<RecommendedBook>()
            .Where(b => b.IsPublished)
            .Include(b => b.Translations).Include(b => b.CoverImage)
            .OrderByDescending(b => b.IsFeatured).ThenBy(b => b.SortOrder)
            .Take(4).ToListAsync();

        FeaturedEvents = await Db.ContentItem.OfType<Event>()
            .Where(e => e.IsPublished)
            .Include(e => e.Translations).Include(e => e.CoverImage)
            .OrderByDescending(e => e.IsFeatured).ThenByDescending(e => e.EventDateUtc)
            .Take(4).ToListAsync();
    }

    /// <summary>Shared clickable-card view model for an Academic Book (doc 82).</summary>
    public BookCardViewModel CardForBook(Book b)
    {
        var t = Localized.Pick(b.Translations, Culture);
        return new BookCardViewModel
        {
            Culture = Culture, DetailPage = "/Public/BookDetail", Slug = t?.Slug,
            CoverPath = b.CoverImage is null ? null : "/uploads/" + b.CoverImage.RelativePath,
            Title = t?.Title, Subtitle = t?.Publisher, Featured = b.IsFeatured,
        };
    }

    /// <summary>Shared clickable-card view model for a Recommended Book (doc 82).</summary>
    public BookCardViewModel CardForRecommended(RecommendedBook b)
    {
        var t = Localized.Pick(b.Translations, Culture);
        return new BookCardViewModel
        {
            Culture = Culture, DetailPage = "/Public/RecommendedBookDetail", Slug = t?.Slug,
            CoverPath = b.CoverImage is null ? null : "/uploads/" + b.CoverImage.RelativePath,
            Title = t?.Title, Subtitle = t?.Authors, Featured = b.IsFeatured,
        };
    }
}
