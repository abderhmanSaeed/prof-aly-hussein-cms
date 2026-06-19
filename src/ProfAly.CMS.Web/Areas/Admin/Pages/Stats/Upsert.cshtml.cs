using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Stats;

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

        [Range(0, int.MaxValue, ErrorMessage = "Field_Year_Range")]
        public int Value { get; set; }

        public string? Suffix { get; set; }

        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Label { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        StatItem? entity = null;
        if (id is int x)
        {
            entity = await _db.StatItem.Include(s => s.Translations).FirstOrDefaultAsync(s => s.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.Value = entity.Value;
            Input.Suffix = entity.Suffix;
        }

        Input.Translations = Cultures.Select(c => new TranslationInput
        {
            Culture = c,
            Label = entity?.Translations.FirstOrDefault(t => t.Culture == c)?.Label,
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.Label))
        {
            ModelState.AddModelError("Input.Translations[0].Label", _t["Field_Required"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        StatItem entity;
        if (Input.Id == 0)
        {
            entity = new StatItem
            {
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.StatItem.MaxAsync(s => (int?)s.SortOrder) ?? -1) + 1,
            };
            _db.StatItem.Add(entity);
        }
        else
        {
            entity = await _db.StatItem.Include(s => s.Translations).FirstAsync(s => s.Id == Input.Id);
        }

        entity.Value = Input.Value;
        entity.Suffix = string.IsNullOrWhiteSpace(Input.Suffix) ? null : Input.Suffix.Trim();
        entity.ModifiedUtc = DateTime.UtcNow;

        foreach (var input in Input.Translations)
        {
            var isDefault = input.Culture == SupportedCultures.Default;
            var hasContent = !string.IsNullOrWhiteSpace(input.Label);
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
                tr = new StatItemTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.Label = input.Label!.Trim();
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
