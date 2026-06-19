using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Infrastructure.Storage;
using ProfAly.CMS.Web.Infrastructure;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Theses;

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
    public string? PdfPath { get; private set; }
    public IReadOnlyList<string> Cultures => SupportedCultures.All;

    public class InputModel
    {
        public int Id { get; set; }
        public RelationshipType RelationshipType { get; set; } = RelationshipType.Supervised;
        public DegreeLevel DegreeLevel { get; set; } = DegreeLevel.Master;
        public int? PublicationYear { get; set; }
        public bool IsPublished { get; set; } = true;
        public IFormFile? PdfFile { get; set; }
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? ResearcherName { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Body { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Thesis? entity = null;
        if (id is int x)
        {
            entity = await _db.ContentItem.OfType<Thesis>().Include(t => t.Translations).Include(t => t.PdfFile)
                .FirstOrDefaultAsync(t => t.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.RelationshipType = entity.RelationshipType;
            Input.DegreeLevel = entity.DegreeLevel;
            Input.PublicationYear = entity.PublicationYear;
            Input.IsPublished = entity.IsPublished;
            PdfPath = entity.PdfFile is null ? null : "/uploads/" + entity.PdfFile.RelativePath;
        }

        Input.Translations = Cultures.Select(c =>
        {
            var tr = entity?.Translations.FirstOrDefault(t => t.Culture == c);
            return new TranslationInput { Culture = c, ResearcherName = tr?.ResearcherName, Title = tr?.Title, Summary = tr?.Summary, Body = tr?.Body };
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
        if (string.IsNullOrWhiteSpace(def.ResearcherName))
        {
            ModelState.AddModelError("Input.Translations[0].ResearcherName", _t["Field_Required"]);
        }

        Thesis entity;
        if (Input.Id == 0)
        {
            entity = new Thesis
            {
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.ContentItem.OfType<Thesis>().MaxAsync(t => (int?)t.SortOrder) ?? -1) + 1,
            };
        }
        else
        {
            entity = await _db.ContentItem.OfType<Thesis>().Include(t => t.Translations).FirstAsync(t => t.Id == Input.Id);
        }

        var pdfId = entity.PdfFileId;
        if (Input.PdfFile is not null)
        {
            var r = await _media.UploadAsync(Input.PdfFile, MediaKind.Pdf);
            if (!r.Succeeded) { ModelState.AddModelError("Input.PdfFile", _t[r.ErrorKey!]); }
            else { pdfId = r.File!.Id; }
        }

        if (!ModelState.IsValid)
        {
            PdfPath = pdfId is null ? null : "/uploads/" + (await _db.MediaFile.Where(m => m.Id == pdfId).Select(m => m.RelativePath).FirstOrDefaultAsync());
            return Page();
        }

        if (Input.Id == 0)
        {
            _db.ContentItem.Add(entity);
        }

        entity.RelationshipType = Input.RelationshipType;
        entity.DegreeLevel = Input.DegreeLevel;
        entity.PublicationYear = Input.PublicationYear;
        entity.IsPublished = Input.IsPublished;
        entity.PdfFileId = pdfId;
        entity.ModifiedUtc = DateTime.UtcNow;

        foreach (var input in Input.Translations)
        {
            var isDefault = input.Culture == SupportedCultures.Default;
            var hasContent = !string.IsNullOrWhiteSpace(input.Title) || !string.IsNullOrWhiteSpace(input.ResearcherName);
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

            tr.Title = (input.Title ?? string.Empty).Trim();
            tr.ResearcherName = string.IsNullOrWhiteSpace(input.ResearcherName) ? null : input.ResearcherName.Trim();
            tr.Summary = string.IsNullOrWhiteSpace(input.Summary) ? null : input.Summary.Trim();
            tr.Body = string.IsNullOrWhiteSpace(input.Body) ? null : input.Body;
            tr.Slug = await EnsureUniqueSlugAsync(SlugHelper.Slugify(input.Title), input.Culture, entity.Id);
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, string culture, int contentItemId)
    {
        var slug = string.IsNullOrEmpty(baseSlug) ? "thesis" : baseSlug;
        var candidate = slug;
        var n = 2;
        while (await _db.ContentItemTranslation.AnyAsync(t => t.Culture == culture && t.Slug == candidate && t.ContentItemId != contentItemId))
        {
            candidate = $"{slug}-{n++}";
        }

        return candidate;
    }
}
