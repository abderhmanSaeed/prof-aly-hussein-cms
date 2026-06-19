using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Memberships;

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
        public MembershipKind Kind { get; set; } = MembershipKind.Society;
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Membership? entity = null;
        if (id is int x)
        {
            entity = await _db.Membership.Include(m => m.Translations).FirstOrDefaultAsync(m => m.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            Input.Kind = entity.Kind;
        }

        Input.Translations = Cultures.Select(c => new TranslationInput
        {
            Culture = c,
            Name = entity?.Translations.FirstOrDefault(t => t.Culture == c)?.Name,
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

        Membership entity;
        if (Input.Id == 0)
        {
            entity = new Membership
            {
                Kind = Input.Kind,
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.Membership.Where(m => m.Kind == Input.Kind).MaxAsync(m => (int?)m.SortOrder) ?? -1) + 1,
            };
            _db.Membership.Add(entity);
        }
        else
        {
            entity = await _db.Membership.Include(m => m.Translations).FirstAsync(m => m.Id == Input.Id);
            entity.Kind = Input.Kind;
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
                tr = new MembershipTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.Name = input.Name!.Trim();
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
