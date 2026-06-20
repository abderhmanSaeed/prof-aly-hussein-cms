using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class BooksModel : PublicPageModel
{
    private const int PageSize = 12;

    public BooksModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageNum { get; set; } = 1;

    /// <summary>Free-text search across title / description / author / publisher (any culture).</summary>
    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Query { get; set; }

    public List<Book> Books { get; private set; } = new();
    public int TotalPages { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();
        var query = Db.ContentItem.OfType<Book>().Where(b => b.IsPublished);

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

        Books = await query
            .Include(b => b.Translations).Include(b => b.CoverImage)
            .OrderBy(b => b.SortOrder).ThenByDescending(b => b.PublicationYear)
            .Skip((PageNum - 1) * PageSize).Take(PageSize)
            .ToListAsync();
    }
}
