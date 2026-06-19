using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Infrastructure.Storage;
using CredibilityEntity = ProfAly.CMS.Domain.Entities.Credibility;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Credibility;

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
    public string? LogoPath { get; private set; }
    public IReadOnlyList<string> Cultures => SupportedCultures.All;

    public class InputModel
    {
        public int Id { get; set; }
        public IFormFile? LogoFile { get; set; }
        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        CredibilityEntity? entity = null;
        if (id is int x)
        {
            entity = await _db.Credibility.Include(c => c.Translations).Include(c => c.Logo).FirstOrDefaultAsync(c => c.Id == x);
            if (entity is null)
            {
                return NotFound();
            }

            Input.Id = entity.Id;
            LogoPath = entity.Logo is null ? null : "/uploads/" + entity.Logo.RelativePath;
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
            await SetLogoPathAsync();
            return Page();
        }

        CredibilityEntity entity;
        if (Input.Id == 0)
        {
            entity = new CredibilityEntity
            {
                CreatedUtc = DateTime.UtcNow,
                SortOrder = (await _db.Credibility.MaxAsync(c => (int?)c.SortOrder) ?? -1) + 1,
            };
            _db.Credibility.Add(entity);
        }
        else
        {
            entity = await _db.Credibility.Include(c => c.Translations).FirstAsync(c => c.Id == Input.Id);
        }

        entity.ModifiedUtc = DateTime.UtcNow;

        if (Input.LogoFile is not null)
        {
            var result = await _media.UploadAsync(Input.LogoFile, MediaKind.Image);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Input.LogoFile", _t[result.ErrorKey!]);
                await SetLogoPathAsync();
                return Page();
            }
            entity.LogoMediaId = result.File!.Id;
        }

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
                tr = new CredibilityTranslation { Culture = input.Culture };
                entity.Translations.Add(tr);
            }

            tr.Name = input.Name!.Trim();
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private async Task SetLogoPathAsync()
    {
        if (Input.Id != 0)
        {
            var logo = await _db.Credibility.Where(c => c.Id == Input.Id).Select(c => c.Logo).FirstOrDefaultAsync();
            LogoPath = logo is null ? null : "/uploads/" + logo.RelativePath;
        }
    }
}
