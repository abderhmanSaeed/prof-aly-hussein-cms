using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Web.Infrastructure;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Videos;

public class UpsertModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResource> _t;

    public UpsertModel(AppDbContext db, IStringLocalizer<SharedResource> t)
    {
        _db = db;
        _t = t;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool IsEdit => Input.Id != 0;
    public string? PreviewVideoId { get; private set; }
    public IReadOnlyList<string> Cultures => SupportedCultures.All;

    public class InputModel
    {
        public int Id { get; set; }
        public string YouTubeUrl { get; set; } = string.Empty;
        public string? PublishDate { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; } = true;
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Video? entity = null;
        if (id is int x)
        {
            entity = await _db.ContentItem.OfType<Video>().Include(v => v.Translations).FirstOrDefaultAsync(v => v.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.YouTubeUrl = string.IsNullOrEmpty(entity.ExternalUrl) ? YouTube.WatchUrl(entity.YouTubeVideoId) : entity.ExternalUrl;
            Input.PublishDate = entity.EventDateUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            Input.IsFeatured = entity.IsFeatured;
            Input.IsPublished = entity.IsPublished;
            PreviewVideoId = entity.YouTubeVideoId;
        }

        Input.Translations = Cultures.Select(c =>
        {
            var tr = entity?.Translations.FirstOrDefault(t => t.Culture == c);
            return new TranslationInput
            {
                Culture = c,
                Title = tr?.Title,
                Description = tr?.Summary,
                MetaTitle = tr?.MetaTitle,
                MetaDescription = tr?.MetaDescription,
                MetaKeywords = tr?.MetaKeywords,
            };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.Title))
        {
            ModelState.AddModelError("Input.Translations[0].Title", _t["Field_Required"]);
        }

        if (!YouTube.TryGetVideoId(Input.YouTubeUrl, out var videoId))
        {
            ModelState.AddModelError("Input.YouTubeUrl", _t["YouTube_Invalid"]);
        }

        if (!ModelState.IsValid)
        {
            PreviewVideoId = videoId;
            return Page();
        }

        Video entity;
        if (Input.Id == 0)
        {
            entity = new Video
            {
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.ContentItem.OfType<Video>().MaxAsync(v => (int?)v.SortOrder) ?? -1) + 1,
            };
            _db.ContentItem.Add(entity);
        }
        else
        {
            entity = await _db.ContentItem.OfType<Video>().Include(v => v.Translations).FirstAsync(v => v.Id == Input.Id);
        }

        entity.YouTubeVideoId = videoId;
        entity.ExternalUrl = YouTube.WatchUrl(videoId);
        entity.EventDateUtc = DateTime.TryParse(Input.PublishDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : null;
        entity.IsFeatured = Input.IsFeatured;
        entity.IsPublished = Input.IsPublished;
        entity.ModifiedUtc = DateTime.UtcNow;

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
            tr.Summary = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
            tr.MetaTitle = string.IsNullOrWhiteSpace(input.MetaTitle) ? null : input.MetaTitle.Trim();
            tr.MetaDescription = string.IsNullOrWhiteSpace(input.MetaDescription) ? null : input.MetaDescription.Trim();
            tr.MetaKeywords = string.IsNullOrWhiteSpace(input.MetaKeywords) ? null : input.MetaKeywords.Trim();
            tr.Slug = await EnsureUniqueSlugAsync(SlugHelper.Slugify(input.Title), input.Culture, entity.Id);
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, string culture, int contentItemId)
    {
        var slug = string.IsNullOrEmpty(baseSlug) ? "video" : baseSlug;
        var candidate = slug;
        var n = 2;
        while (await _db.ContentItemTranslation.AnyAsync(t => t.Culture == culture && t.Slug == candidate && t.ContentItemId != contentItemId))
        {
            candidate = $"{slug}-{n++}";
        }

        return candidate;
    }
}
