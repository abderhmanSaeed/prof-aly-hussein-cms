using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Qualifications;

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

        [Range(ContentRules.MinPublicationYear, 2100, ErrorMessage = "Field_Year_Range")]
        public int? Year { get; set; }

        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public string? Institution { get; set; }
        public string? Grade { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Qualification? entity = null;
        if (id is int x)
        {
            entity = await _db.Qualification.Include(q => q.Translations).FirstOrDefaultAsync(q => q.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.Year = entity.Year;
        }

        Input.Translations = Cultures.Select(c =>
        {
            var tr = entity?.Translations.FirstOrDefault(t => t.Culture == c);
            return new TranslationInput { Culture = c, Degree = tr?.Degree, Institution = tr?.Institution, Grade = tr?.Grade };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.Degree))
        {
            ModelState.AddModelError("Input.Translations[0].Degree", _t["Field_Required"]);
        }
        if (string.IsNullOrWhiteSpace(def.Institution))
        {
            ModelState.AddModelError("Input.Translations[0].Institution", _t["Field_Required"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Qualification entity;
        if (Input.Id == 0)
        {
            entity = new Qualification
            {
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.Qualification.MaxAsync(q => (int?)q.SortOrder) ?? -1) + 1,
            };
            _db.Qualification.Add(entity);
        }
        else
        {
            entity = await _db.Qualification.Include(q => q.Translations).FirstAsync(q => q.Id == Input.Id);
        }

        entity.Year = Input.Year;
        entity.ModifiedUtc = DateTime.UtcNow;

        foreach (var input in Input.Translations)
        {
            var isDefault = input.Culture == SupportedCultures.Default;
            var hasContent = !string.IsNullOrWhiteSpace(input.Degree) || !string.IsNullOrWhiteSpace(input.Institution);
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
                tr = new QualificationTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.Degree = (input.Degree ?? string.Empty).Trim();
            tr.Institution = (input.Institution ?? string.Empty).Trim();
            tr.Grade = string.IsNullOrWhiteSpace(input.Grade) ? null : input.Grade.Trim();
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
