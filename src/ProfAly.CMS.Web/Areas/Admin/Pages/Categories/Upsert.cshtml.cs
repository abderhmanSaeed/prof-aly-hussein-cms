using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Web.Infrastructure;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Categories;

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
    public IReadOnlyList<string> Cultures => SupportedCultures.All;

    public class InputModel
    {
        public int Id { get; set; }
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Slug { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Category? entity = null;
        if (id is int x)
        {
            entity = await _db.Category.Include(c => c.Translations).FirstOrDefaultAsync(c => c.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
        }

        Input.Translations = Cultures.Select(c =>
        {
            var tr = entity?.Translations.FirstOrDefault(t => t.Culture == c);
            return new TranslationInput { Culture = c, Name = tr?.Name, Slug = tr?.Slug };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.Name))
        {
            ModelState.AddModelError("Input.Translations[0].Name", _t["Field_Required"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Category entity;
        if (Input.Id == 0)
        {
            entity = new Category
            {
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.Category.MaxAsync(c => (int?)c.SortOrder) ?? -1) + 1,
            };
            _db.Category.Add(entity);
        }
        else
        {
            entity = await _db.Category.Include(c => c.Translations).FirstAsync(c => c.Id == Input.Id);
        }

        entity.ModifiedUtc = DateTime.UtcNow;

        foreach (var input in Input.Translations)
        {
            var isDefault = input.Culture == SupportedCultures.Default;
            var hasContent = !string.IsNullOrWhiteSpace(input.Name);
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
                tr = new CategoryTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.Name = input.Name!.Trim();
            var baseSlug = !string.IsNullOrWhiteSpace(input.Slug) ? SlugHelper.Slugify(input.Slug) : SlugHelper.Slugify(input.Name);
            tr.Slug = await EnsureUniqueSlugAsync(baseSlug, input.Culture, entity.Id);
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, string culture, int categoryId)
    {
        var slug = string.IsNullOrEmpty(baseSlug) ? "category" : baseSlug;
        var candidate = slug;
        var n = 2;
        while (await _db.CategoryTranslation.AnyAsync(t =>
                   t.Culture == culture && t.Slug == candidate && t.CategoryId != categoryId))
        {
            candidate = $"{slug}-{n++}";
        }

        return candidate;
    }
}
