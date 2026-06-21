using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Web.Infrastructure;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class RecommendedBooksModel : PublicPageModel
{
    private const int PageSize = 12;

    public RecommendedBooksModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Query { get; set; }

    public List<RecommendedBook> Featured { get; private set; } = new();
    public List<RecommendedBook> Items { get; private set; } = new();
    public int TotalPages { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();

        var query = Db.ContentItem.OfType<RecommendedBook>().Where(b => b.IsPublished);

        var q = Query?.Trim();
        if (!string.IsNullOrEmpty(q))
        {
            var like = $"%{q}%";
            query = query.Where(b => b.Translations.Any(t =>
                EF.Functions.Like(t.Title, like)
                || (t.Summary != null && EF.Functions.Like(t.Summary, like))
                || (t.Authors != null && EF.Functions.Like(t.Authors, like))
                || (t.Publisher != null && EF.Functions.Like(t.Publisher, like))));
        }

        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (PageNum < 1) { PageNum = 1; }

        if (PageNum == 1 && string.IsNullOrEmpty(q))
        {
            Featured = await query.Where(b => b.IsFeatured)
                .Include(b => b.Translations).Include(b => b.CoverImage)
                .OrderBy(b => b.SortOrder).Take(3).ToListAsync();
        }

        Items = await query
            .Include(b => b.Translations).Include(b => b.CoverImage)
            .OrderBy(b => b.SortOrder).ThenBy(b => b.Id)
            .Skip((PageNum - 1) * PageSize).Take(PageSize)
            .ToListAsync();
    }

    /// <summary>Maps a RecommendedBook to the shared clickable-card view model (doc 82).</summary>
    public BookCardViewModel CardFor(RecommendedBook b)
    {
        var t = Localized.Pick(b.Translations, Culture);
        return new BookCardViewModel
        {
            Culture = Culture,
            DetailPage = "/Public/RecommendedBookDetail",
            Slug = t?.Slug,
            CoverPath = b.CoverImage is null ? null : "/uploads/" + b.CoverImage.RelativePath,
            Title = t?.Title,
            Subtitle = t?.Authors,
            SubMeta = t?.Publisher,
            Featured = b.IsFeatured,
        };
    }
}
