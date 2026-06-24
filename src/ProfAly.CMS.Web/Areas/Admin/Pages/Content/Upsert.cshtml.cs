using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Infrastructure.Storage;
using ProfAly.CMS.Web.Infrastructure;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Content;

public class UpsertModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IMediaUploadService _media;
    private readonly IStringLocalizer<SharedResource> _t;

    public UpsertModel(AppDbContext db, IMediaUploadService media, IStringLocalizer<SharedResource> t)
    {
        _db = db;
        _media = media;
        _t = t;
    }

    [BindProperty(SupportsGet = true)]
    public string Type { get; set; } = "Book";

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public ContentType ContentType { get; private set; }
    public bool IsBook => ContentType == ContentType.Book;
    public bool IsPaper => ContentType is ContentType.Publication or ContentType.ResearchPaper;
    public bool IsEnrichment => ContentType == ContentType.EnrichmentItem;
    public bool IsRecommendedBook => ContentType == ContentType.RecommendedBook;
    public bool IsEdit => Input.Id != 0;
    public IReadOnlyList<string> Cultures => SupportedCultures.All;
    public string? CoverPath { get; private set; }
    public string? PdfPath { get; private set; }
    public List<(int Id, string Name)> AllCategories { get; private set; } = new();

    public string TitleKey => ContentType switch
    {
        ContentType.Publication => "Publications_Title",
        ContentType.ResearchPaper => "Research_Title",
        ContentType.EnrichmentItem => "Enrichment_Title",
        ContentType.RecommendedBook => "RecommendedBooks_Title",
        _ => "Books_Title",
    };

    public class InputModel
    {
        public int Id { get; set; }
        public int? PublicationYear { get; set; }
        public string? ExternalUrl { get; set; }
        public string? Doi { get; set; }
        public string? PurchaseUrl { get; set; }
        public bool IsPublished { get; set; }
        public bool IsFeatured { get; set; }
        public List<int> CategoryIds { get; set; } = new();
        public IFormFile? CoverFile { get; set; }
        public IFormFile? PdfFile { get; set; }
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Summary { get; set; }
        public string? Body { get; set; }
        public string? Journal { get; set; }
        public string? Authors { get; set; }
        public string? Publisher { get; set; }
        public string? AuthorshipRole { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!TryResolveType(out var ct))
        {
            return NotFound();
        }

        ContentType = ct;
        await LoadCategoriesAsync();

        ContentItem? entity = null;
        if (id is int x)
        {
            entity = await Query().FirstOrDefaultAsync(c => c.Id == x && c.ContentType == ct);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.PublicationYear = entity.PublicationYear;
            Input.ExternalUrl = entity.ExternalUrl;
            Input.IsPublished = entity.IsPublished;
            Input.IsFeatured = entity.IsFeatured;
            Input.Doi = entity switch { Publication p => p.Doi, ResearchPaper r => r.Doi, _ => null };
            Input.PurchaseUrl = entity is RecommendedBook rb ? rb.PurchaseUrl : null;
            Input.CategoryIds = entity.Categories.Select(c => c.CategoryId).ToList();
            CoverPath = entity.CoverImage is null ? null : "/uploads/" + entity.CoverImage.RelativePath;
            PdfPath = entity.PdfFile is null ? null : "/uploads/" + entity.PdfFile.RelativePath;
        }

        Input.Translations = Cultures.Select(c =>
        {
            var tr = entity?.Translations.FirstOrDefault(t => t.Culture == c);
            return new TranslationInput
            {
                Culture = c,
                Title = tr?.Title, Slug = tr?.Slug, Summary = tr?.Summary, Body = tr?.Body,
                Journal = tr?.Journal, Authors = tr?.Authors, Publisher = tr?.Publisher, AuthorshipRole = tr?.AuthorshipRole,
                MetaTitle = tr?.MetaTitle, MetaDescription = tr?.MetaDescription, MetaKeywords = tr?.MetaKeywords,
            };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!TryResolveType(out var ct))
        {
            return NotFound();
        }

        ContentType = ct;
        await LoadCategoriesAsync();

        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.Title))
        {
            ModelState.AddModelError("Input.Translations[0].Title", _t["Field_Required"]);
        }

        ContentItem entity;
        if (Input.Id == 0)
        {
            entity = NewOfType(ct);
            entity.CreatedUtc = DateTime.UtcNow;
            entity.SortOrder = (await _db.ContentItem.Where(c => c.ContentType == ct).MaxAsync(c => (int?)c.SortOrder) ?? -1) + 1;
        }
        else
        {
            entity = await Query().FirstAsync(c => c.Id == Input.Id && c.ContentType == ct);
        }

        // Media (validate before mutating further so errors short-circuit cleanly).
        var coverId = entity.CoverImageId;
        var pdfId = entity.PdfFileId;
        if (Input.CoverFile is not null)
        {
            var r = await _media.UploadAsync(Input.CoverFile, MediaKind.Image);
            if (!r.Succeeded) { ModelState.AddModelError("Input.CoverFile", _t[r.ErrorKey!]); }
            else { coverId = r.File!.Id; }
        }
        if (Input.PdfFile is not null)
        {
            // Enrichment Items accept PDF + Word + PowerPoint; other types stay PDF-only.
            var fileKind = ct == ContentType.EnrichmentItem ? MediaKind.Document : MediaKind.Pdf;
            var r = await _media.UploadAsync(Input.PdfFile, fileKind);
            if (!r.Succeeded) { ModelState.AddModelError("Input.PdfFile", _t[r.ErrorKey!]); }
            else { pdfId = r.File!.Id; }
        }

        if (!ModelState.IsValid)
        {
            CoverPath = await MediaPathAsync(coverId);
            PdfPath = await MediaPathAsync(pdfId);
            return Page();
        }

        if (Input.Id == 0)
        {
            _db.ContentItem.Add(entity);
        }

        entity.PublicationYear = Input.PublicationYear;
        entity.ExternalUrl = string.IsNullOrWhiteSpace(Input.ExternalUrl) ? null : Input.ExternalUrl.Trim();
        entity.IsPublished = Input.IsPublished;
        entity.IsFeatured = Input.IsFeatured;
        entity.CoverImageId = coverId;
        entity.PdfFileId = pdfId;
        entity.ModifiedUtc = DateTime.UtcNow;

        var doi = string.IsNullOrWhiteSpace(Input.Doi) ? null : Input.Doi.Trim();
        switch (entity)
        {
            case Publication p: p.Doi = doi; break;
            case ResearchPaper r: r.Doi = doi; break;
            case RecommendedBook rb: rb.PurchaseUrl = string.IsNullOrWhiteSpace(Input.PurchaseUrl) ? null : Input.PurchaseUrl.Trim(); break;
        }

        await UpsertTranslationsAsync(entity, ct);
        SyncCategories(entity);

        await _db.SaveChangesAsync();
        return RedirectToPage("Index", new { Type });
    }

    private async Task UpsertTranslationsAsync(ContentItem entity, ContentType ct)
    {
        foreach (var input in Input.Translations)
        {
            var isDefault = input.Culture == SupportedCultures.Default;
            var hasContent = !string.IsNullOrWhiteSpace(input.Title);
            var tr = entity.Translations.FirstOrDefault(t => t.Culture == input.Culture);

            if (!isDefault && !hasContent)
            {
                if (tr is not null)
                {
                    entity.Translations.Remove(tr);
                }
                continue;
            }

            if (tr is null)
            {
                tr = new ContentItemTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.Title = input.Title!.Trim();
            var baseSlug = !string.IsNullOrWhiteSpace(input.Slug) ? SlugHelper.Slugify(input.Slug) : SlugHelper.Slugify(input.Title);
            tr.Slug = await EnsureUniqueSlugAsync(baseSlug, input.Culture, entity.Id);
            tr.Summary = Trim(input.Summary);
            tr.Body = string.IsNullOrWhiteSpace(input.Body) ? null : input.Body;
            tr.MetaTitle = Trim(input.MetaTitle);
            tr.MetaDescription = Trim(input.MetaDescription);
            tr.MetaKeywords = Trim(input.MetaKeywords);

            // Type-specific translatable fields. Each type populates only its own and
            // nulls the rest so a type change can't leave stale text behind.
            switch (ct)
            {
                case ContentType.Book:
                    tr.Publisher = Trim(input.Publisher);
                    tr.AuthorshipRole = Trim(input.AuthorshipRole);
                    tr.Journal = null;
                    tr.Authors = null;
                    break;
                case ContentType.RecommendedBook:
                    tr.Authors = Trim(input.Authors);      // "Author" field
                    tr.Publisher = Trim(input.Publisher);
                    tr.Journal = null;
                    tr.AuthorshipRole = null;
                    break;
                case ContentType.EnrichmentItem:
                    tr.Journal = null;
                    tr.Authors = null;
                    tr.Publisher = null;
                    tr.AuthorshipRole = null;
                    break;
                default: // Publication / ResearchPaper
                    tr.Journal = Trim(input.Journal);
                    tr.Authors = Trim(input.Authors);
                    tr.Publisher = null;
                    tr.AuthorshipRole = null;
                    break;
            }
        }
    }

    private void SyncCategories(ContentItem entity)
    {
        var selected = Input.CategoryIds.Distinct().ToHashSet();
        foreach (var link in entity.Categories.Where(c => !selected.Contains(c.CategoryId)).ToList())
        {
            entity.Categories.Remove(link);
        }

        var existing = entity.Categories.Select(c => c.CategoryId).ToHashSet();
        foreach (var categoryId in selected.Where(id => !existing.Contains(id)))
        {
            entity.Categories.Add(new ContentItemCategory { CategoryId = categoryId });
        }
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, string culture, int contentItemId)
    {
        var slug = string.IsNullOrEmpty(baseSlug) ? "item" : baseSlug;
        var candidate = slug;
        var n = 2;
        while (await _db.ContentItemTranslation.AnyAsync(t =>
                   t.Culture == culture && t.Slug == candidate && t.ContentItemId != contentItemId))
        {
            candidate = $"{slug}-{n++}";
        }

        return candidate;
    }

    private IQueryable<ContentItem> Query() => _db.ContentItem
        .Include(c => c.Translations)
        .Include(c => c.Categories)
        .Include(c => c.CoverImage)
        .Include(c => c.PdfFile);

    private async Task LoadCategoriesAsync()
    {
        var categories = await _db.Category.Include(c => c.Translations)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id).ToListAsync();
        AllCategories = categories.Select(c => (
            c.Id,
            (c.Translations.FirstOrDefault(t => t.Culture == SupportedCultures.Default) ?? c.Translations.FirstOrDefault())?.Name ?? $"#{c.Id}"
        )).ToList();
    }

    private async Task<string?> MediaPathAsync(int? mediaId)
    {
        if (mediaId is null)
        {
            return null;
        }

        var rel = await _db.MediaFile.Where(m => m.Id == mediaId).Select(m => m.RelativePath).FirstOrDefaultAsync();
        return rel is null ? null : "/uploads/" + rel;
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ContentItem NewOfType(ContentType ct) => ct switch
    {
        ContentType.Publication => new Publication(),
        ContentType.ResearchPaper => new ResearchPaper(),
        ContentType.EnrichmentItem => new EnrichmentItem(),
        ContentType.RecommendedBook => new RecommendedBook(),
        _ => new Book(),
    };

    private bool TryResolveType(out ContentType ct)
    {
        ct = default;
        return Enum.TryParse(Type, out ct) && IndexModel.Allowed.Contains(ct);
    }
}
