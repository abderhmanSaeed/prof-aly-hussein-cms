using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Teaching;

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
        public CourseLevel Level { get; set; } = CourseLevel.Undergraduate;
        public string? Period { get; set; }
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public string? Institution { get; set; }
        public string? Description { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Course? entity = null;
        if (id is int x)
        {
            entity = await _db.Course.Include(c => c.Translations).FirstOrDefaultAsync(c => c.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.Level = entity.Level;
            Input.Period = entity.Period;
        }

        Input.Translations = Cultures.Select(c =>
        {
            var tr = entity?.Translations.FirstOrDefault(t => t.Culture == c);
            return new TranslationInput { Culture = c, CourseName = tr?.CourseName, Institution = tr?.Institution, Description = tr?.Description };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.CourseName))
        {
            ModelState.AddModelError("Input.Translations[0].CourseName", _t["Field_Required"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Course entity;
        if (Input.Id == 0)
        {
            entity = new Course
            {
                Level = Input.Level,
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.Course.Where(c => c.Level == Input.Level).MaxAsync(c => (int?)c.SortOrder) ?? -1) + 1,
            };
            _db.Course.Add(entity);
        }
        else
        {
            entity = await _db.Course.Include(c => c.Translations).FirstAsync(c => c.Id == Input.Id);
            entity.Level = Input.Level;
        }

        entity.Period = string.IsNullOrWhiteSpace(Input.Period) ? null : Input.Period.Trim();
        entity.ModifiedUtc = DateTime.UtcNow;

        foreach (var input in Input.Translations)
        {
            var isDefault = input.Culture == SupportedCultures.Default;
            var hasContent = !string.IsNullOrWhiteSpace(input.CourseName);
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
                tr = new CourseTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.CourseName = input.CourseName!.Trim();
            tr.Institution = string.IsNullOrWhiteSpace(input.Institution) ? null : input.Institution.Trim();
            tr.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
