using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Web.Infrastructure;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class BookDetailModel : PublicPageModel
{
    public BookDetailModel(AppDbContext db) : base(db) { }

    [BindProperty(SupportsGet = true)]
    public string Slug { get; set; } = string.Empty;

    public Book? Book { get; private set; }
    public BookDetailViewModel Detail { get; private set; } = new();
    public List<Book> Related { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadChromeAsync();

        Book = await Db.ContentItem.OfType<Book>()
            .Include(b => b.Translations)
            .Include(b => b.CoverImage)
            .Include(b => b.PdfFile)
            .Include(b => b.Categories).ThenInclude(c => c.Category).ThenInclude(c => c!.Translations)
            .Where(b => b.IsPublished && b.Translations.Any(t => t.Slug == Slug))
            .FirstOrDefaultAsync();

        if (Book is null)
        {
            return NotFound();
        }

        var t = Localized.Pick(Book.Translations, Culture);
        Detail = new BookDetailViewModel
        {
            CoverPath = Book.CoverImage is null ? null : "/uploads/" + Book.CoverImage.RelativePath,
            Title = t?.Title,
            Author = t?.Authors,
            Publisher = t?.Publisher,
            AuthorshipRole = t?.AuthorshipRole,
            Year = Book.PublicationYear,
            Categories = Book.Categories
                .Select(c => Localized.Pick(c.Category?.Translations, Culture)?.Name)
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => n!)
                .ToList(),
            Summary = t?.Summary,
            Body = t?.Body,
            PdfPath = Book.PdfFile is null ? null : "/uploads/" + Book.PdfFile.RelativePath,
            PdfName = Book.PdfFile?.OriginalFileName,
            ExternalUrl = Book.ExternalUrl,
            PurchaseUrl = null, // Academic Books have no purchase link.
        };

        Related = await Db.ContentItem.OfType<Book>()
            .Where(b => b.IsPublished && b.Id != Book.Id)
            .Include(b => b.Translations).Include(b => b.CoverImage)
            .OrderByDescending(b => b.IsFeatured).ThenBy(b => b.SortOrder)
            .Take(4).ToListAsync();

        return Page();
    }

    public BookCardViewModel CardFor(Book b)
    {
        var t = Localized.Pick(b.Translations, Culture);
        return new BookCardViewModel
        {
            Culture = Culture,
            DetailPage = "/Public/BookDetail",
            Slug = t?.Slug,
            CoverPath = b.CoverImage is null ? null : "/uploads/" + b.CoverImage.RelativePath,
            Title = t?.Title,
            Subtitle = t?.Publisher,
            Featured = b.IsFeatured,
        };
    }
}
