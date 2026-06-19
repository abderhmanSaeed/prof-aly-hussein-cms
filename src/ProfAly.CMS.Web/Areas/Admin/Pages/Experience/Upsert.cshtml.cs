using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Experience;

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
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Organization { get; set; }
        public string? Description { get; set; }
        public string? PeriodLabel { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        ExperienceEntry? entity = null;
        if (id is int x)
        {
            entity = await _db.ExperienceEntry.Include(e => e.Translations).FirstOrDefaultAsync(e => e.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.StartDate = entity.StartDateUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            Input.EndDate = entity.EndDateUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        Input.Translations = Cultures.Select(c =>
        {
            var tr = entity?.Translations.FirstOrDefault(t => t.Culture == c);
            return new TranslationInput { Culture = c, Role = tr?.Role, Organization = tr?.Organization, Description = tr?.Description, PeriodLabel = tr?.PeriodLabel };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.Role))
        {
            ModelState.AddModelError("Input.Translations[0].Role", _t["Field_Required"]);
        }
        if (string.IsNullOrWhiteSpace(def.Organization))
        {
            ModelState.AddModelError("Input.Translations[0].Organization", _t["Field_Required"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        ExperienceEntry entity;
        if (Input.Id == 0)
        {
            entity = new ExperienceEntry
            {
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.ExperienceEntry.MaxAsync(e => (int?)e.SortOrder) ?? -1) + 1,
            };
            _db.ExperienceEntry.Add(entity);
        }
        else
        {
            entity = await _db.ExperienceEntry.Include(e => e.Translations).FirstAsync(e => e.Id == Input.Id);
        }

        entity.StartDateUtc = ParseDate(Input.StartDate);
        entity.EndDateUtc = ParseDate(Input.EndDate);
        entity.ModifiedUtc = DateTime.UtcNow;

        foreach (var input in Input.Translations)
        {
            var isDefault = input.Culture == SupportedCultures.Default;
            var hasContent = !string.IsNullOrWhiteSpace(input.Role) || !string.IsNullOrWhiteSpace(input.Organization);
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
                tr = new ExperienceEntryTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.Role = (input.Role ?? string.Empty).Trim();
            tr.Organization = (input.Organization ?? string.Empty).Trim();
            tr.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
            tr.PeriodLabel = string.IsNullOrWhiteSpace(input.PeriodLabel) ? null : input.PeriodLabel.Trim();
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private static DateTime? ParseDate(string? value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
}
