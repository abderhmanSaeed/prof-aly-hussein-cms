using System.Globalization;
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

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Events;

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

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool IsEdit => Input.Id != 0;
    public IReadOnlyList<string> Cultures => SupportedCultures.All;
    public string? CoverPath { get; private set; }
    public List<(int Id, string Name)> AllCategories { get; private set; } = new();
    public List<(int Id, string Path, string? Caption)> ExistingImages { get; private set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        public string? EventDate { get; set; }
        public string? ExternalUrl { get; set; }
        public bool IsPublished { get; set; } = true;
        public bool IsFeatured { get; set; }
        public List<int> CategoryIds { get; set; } = new();
        public IFormFile? CoverFile { get; set; }
        public List<IFormFile>? GalleryFiles { get; set; }
        public List<int> RemoveImageIds { get; set; } = new();
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Location { get; set; }
        public string? Summary { get; set; }
        public string? Body { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadCategoriesAsync();

        Event? entity = null;
        if (id is int x)
        {
            entity = await Query().FirstOrDefaultAsync(e => e.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.EventDate = entity.EventDateUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            Input.ExternalUrl = entity.ExternalUrl;
            Input.IsPublished = entity.IsPublished;
            Input.IsFeatured = entity.IsFeatured;
            Input.CategoryIds = entity.Categories.Select(c => c.CategoryId).ToList();
            CoverPath = entity.CoverImage is null ? null : "/uploads/" + entity.CoverImage.RelativePath;
            ExistingImages = entity.Images.OrderBy(i => i.SortOrder)
                .Where(i => i.MediaFile is not null)
                .Select(i => (i.Id, "/uploads/" + i.MediaFile!.RelativePath, i.Caption)).ToList();
        }

        Input.Translations = Cultures.Select(c =>
        {
            var tr = entity?.Translations.FirstOrDefault(t => t.Culture == c);
            return new TranslationInput
            {
                Culture = c,
                Title = tr?.Title, Slug = tr?.Slug, Location = tr?.Location, Summary = tr?.Summary, Body = tr?.Body,
                MetaTitle = tr?.MetaTitle, MetaDescription = tr?.MetaDescription, MetaKeywords = tr?.MetaKeywords,
            };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();

        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.Title))
        {
            ModelState.AddModelError("Input.Translations[0].Title", _t["Field_Required"]);
        }

        Event entity;
        if (Input.Id == 0)
        {
            entity = new Event
            {
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.ContentItem.OfType<Event>().MaxAsync(e => (int?)e.SortOrder) ?? -1) + 1,
            };
        }
        else
        {
            entity = await Query().FirstAsync(e => e.Id == Input.Id);
        }

        // Cover (validate before mutating so errors short-circuit cleanly).
        var coverId = entity.CoverImageId;
        if (Input.CoverFile is not null)
        {
            var r = await _media.UploadAsync(Input.CoverFile, MediaKind.Image);
            if (!r.Succeeded) { ModelState.AddModelError("Input.CoverFile", _t[r.ErrorKey!]); }
            else { coverId = r.File!.Id; }
        }

        // Validate any new gallery files up-front.
        var uploadedGallery = new List<MediaFile>();
        foreach (var file in Input.GalleryFiles ?? new List<IFormFile>())
        {
            if (file.Length == 0) { continue; }
            var r = await _media.UploadAsync(file, MediaKind.Image);
            if (!r.Succeeded) { ModelState.AddModelError("Input.GalleryFiles", _t[r.ErrorKey!]); }
            else { uploadedGallery.Add(r.File!); }
        }

        if (!ModelState.IsValid)
        {
            CoverPath = await MediaPathAsync(coverId);
            await ReloadExistingImagesAsync();
            return Page();
        }

        if (Input.Id == 0)
        {
            _db.ContentItem.Add(entity);
        }

        entity.EventDateUtc = DateTime.TryParse(Input.EventDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : null;
        entity.ExternalUrl = string.IsNullOrWhiteSpace(Input.ExternalUrl) ? null : Input.ExternalUrl.Trim();
        entity.IsPublished = Input.IsPublished;
        entity.IsFeatured = Input.IsFeatured;
        entity.CoverImageId = coverId;
        entity.ModifiedUtc = DateTime.UtcNow;

        await UpsertTranslationsAsync(entity);
        SyncCategories(entity);
        SyncGallery(entity, uploadedGallery);

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private void SyncGallery(Event entity, List<MediaFile> uploaded)
    {
        // Remove unchecked-for-removal existing images.
        if (Input.RemoveImageIds.Count > 0)
        {
            foreach (var img in entity.Images.Where(i => Input.RemoveImageIds.Contains(i.Id)).ToList())
            {
                entity.Images.Remove(img);
            }
        }

        // Append newly uploaded images at the end.
        var next = entity.Images.Count == 0 ? 0 : entity.Images.Max(i => i.SortOrder) + 1;
        foreach (var media in uploaded)
        {
            entity.Images.Add(new ContentImage { MediaFileId = media.Id, SortOrder = next++ });
        }
    }

    private async Task UpsertTranslationsAsync(Event entity)
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
            tr.Location = Trim(input.Location);
            tr.Summary = Trim(input.Summary);
            tr.Body = string.IsNullOrWhiteSpace(input.Body) ? null : input.Body;
            tr.MetaTitle = Trim(input.MetaTitle);
            tr.MetaDescription = Trim(input.MetaDescription);
            tr.MetaKeywords = Trim(input.MetaKeywords);
            // Event uses no Journal/Authors/Publisher fields.
            tr.Journal = tr.Authors = tr.Publisher = tr.AuthorshipRole = null;
        }
    }

    private void SyncCategories(Event entity)
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
        var slug = string.IsNullOrEmpty(baseSlug) ? "event" : baseSlug;
        var candidate = slug;
        var n = 2;
        while (await _db.ContentItemTranslation.AnyAsync(t =>
                   t.Culture == culture && t.Slug == candidate && t.ContentItemId != contentItemId))
        {
            candidate = $"{slug}-{n++}";
        }

        return candidate;
    }

    private IQueryable<Event> Query() => _db.ContentItem.OfType<Event>()
        .Include(e => e.Translations)
        .Include(e => e.Categories)
        .Include(e => e.CoverImage)
        .Include(e => e.Images).ThenInclude(i => i.MediaFile);

    private async Task LoadCategoriesAsync()
    {
        var categories = await _db.Category.Include(c => c.Translations)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id).ToListAsync();
        AllCategories = categories.Select(c => (
            c.Id,
            (c.Translations.FirstOrDefault(t => t.Culture == SupportedCultures.Default) ?? c.Translations.FirstOrDefault())?.Name ?? $"#{c.Id}"
        )).ToList();
    }

    private async Task ReloadExistingImagesAsync()
    {
        if (Input.Id == 0)
        {
            return;
        }

        ExistingImages = await _db.ContentImage
            .Where(i => i.ContentItemId == Input.Id && i.MediaFile != null)
            .OrderBy(i => i.SortOrder)
            .Select(i => new ValueTuple<int, string, string?>(i.Id, "/uploads/" + i.MediaFile!.RelativePath, i.Caption))
            .ToListAsync();
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
}
