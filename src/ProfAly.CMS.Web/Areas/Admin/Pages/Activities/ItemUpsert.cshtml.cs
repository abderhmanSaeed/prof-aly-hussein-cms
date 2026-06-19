using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Activities;

public class ItemUpsertModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResource> _t;

    public ItemUpsertModel(AppDbContext db, IStringLocalizer<SharedResource> t)
    {
        _db = db;
        _t = t;
    }

    [BindProperty(SupportsGet = true)]
    public int GroupId { get; set; }

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
        public string? Text { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!await _db.ActivityGroup.AnyAsync(g => g.Id == GroupId))
        {
            return NotFound();
        }

        Activity? entity = null;
        if (id is int x)
        {
            entity = await _db.Activity.Include(a => a.Translations).FirstOrDefaultAsync(a => a.Id == x && a.ActivityGroupId == GroupId);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
        }

        Input.Translations = Cultures.Select(c => new TranslationInput
        {
            Culture = c,
            Text = entity?.Translations.FirstOrDefault(t => t.Culture == c)?.Text,
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var def = Input.Translations.First(t => t.Culture == SupportedCultures.Default);
        if (string.IsNullOrWhiteSpace(def.Text))
        {
            ModelState.AddModelError("Input.Translations[0].Text", _t["Field_Required"]);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Activity entity;
        if (Input.Id == 0)
        {
            entity = new Activity
            {
                ActivityGroupId = GroupId,
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.Activity.Where(a => a.ActivityGroupId == GroupId).MaxAsync(a => (int?)a.SortOrder) ?? -1) + 1,
            };
            _db.Activity.Add(entity);
        }
        else
        {
            entity = await _db.Activity.Include(a => a.Translations).FirstAsync(a => a.Id == Input.Id && a.ActivityGroupId == GroupId);
        }

        entity.ModifiedUtc = DateTime.UtcNow;

        foreach (var input in Input.Translations)
        {
            var isDefault = input.Culture == SupportedCultures.Default;
            var hasContent = !string.IsNullOrWhiteSpace(input.Text);
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
                tr = new ActivityTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.Text = input.Text!.Trim();
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Items", new { GroupId });
    }
}
